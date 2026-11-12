using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static smartFactoryApp.Models.Parameters;
using static smartFactoryApp.Models.ViewParameters;

namespace smartFactoryApp.Models;

public class Factory
{
    private Robot robot;
    private ProductionLine productionBlue;
    private ProductionLine productionYellow;
    private ProductionLine productionRed;
    private ProductionLine productionGreen;
    private readonly List<ProductionLine> alleLinien;
    private readonly Logistics logistics = new Logistics();

    // 50 % Qualität: ob der nächste abgedockte Brick defekt ist (→ Recycling), wird zufällig ausgewürfelt.
    private static readonly Random random = new Random();
    private bool naechsterZumRecycling = random.Next(2) == 0;
    // Ergebnis der letzten Auslieferung beim Kunden (null = noch keine)
    private DeliveryResult? lastDelivery;

    public double RobotX => robot.Location.X;
    public double RobotY => robot.Location.Y;
    // Setter wird für ResetRotation gebraucht (Winkel zurück auf 0).
    // Über Drehen, damit ein gedockter Legostein mitgedreht wird.
    public double RobotAngle { get => robot.Angle; set => robot.Drehen(value - robot.Angle); }
    // Richtung der kontinuierlichen Bewegung (Move-Modus), stop = Roboter steht.
    public Direction RobotDirection { get => robot.RobotDirection; set => robot.RobotDirection = value; }

    public List<LegoBrick> YellowBricks => productionYellow.Bricks;
    public List<LegoBrick> RedBricks => productionRed.Bricks;
    public List<LegoBrick> BlueBricks => productionBlue.Bricks;
    public List<LegoBrick> GreenBricks => productionGreen.Bricks;
    public IEnumerable<LegoBrick> AlleBricks => alleLinien.SelectMany(l => l.Bricks);

    // Bestellung und Logistik-Plan
    public bool HasOrder => logistics.HasOrder;
    public bool OrderComplete => logistics.IsComplete;
    public int OrderedTotal => logistics.OrderedTotal;
    public int DeliveredTotal => logistics.DeliveredTotal;
    public DeliveryResult? LastDelivery => lastDelivery;
    public int GetOrdered(LegoColor color) => logistics.GetOrdered(color);
    public int GetDelivered(LegoColor color) => logistics.GetDelivered(color);
    // Nächster Schritt des Plans, immer aus dem aktuellen Zustand berechnet
    public LogisticsStep LogisticsStep => logistics.PickStation(robot.Dockobjekt as LegoBrick, alleLinien);

    public double ProductionYellowX => productionYellow.Location.X;
    public double ProductionYellowY => productionYellow.Location.Y;
    public double ProductionYellowAngle => productionYellow.Angle;

    public double ProductionRedX => productionRed.Location.X;
    public double ProductionRedY => productionRed.Location.Y;
    public double ProductionRedAngle => productionRed.Angle;

    public double ProductionBlueX => productionBlue.Location.X;
    public double ProductionBlueY => productionBlue.Location.Y;
    public double ProductionBlueAngle => productionBlue.Angle;

    public double ProductionGreenX => productionGreen.Location.X;
    public double ProductionGreenY => productionGreen.Location.Y;
    public double ProductionGreenAngle => productionGreen.Angle;

    public Factory()
    {
        robot = new Robot(RobotPose.Location, Borders);

        // Jede Linie bekommt ihre Pose aus ViewParameters und ihre beiden Bewegungsrichtungen.
        // Am Anfang sind alle Linien leer (siehe SetupProductionLines).
        productionYellow = new ProductionLine(RectYellowPose, LegoColor.yellow, Direction.left, Direction.right);
        productionBlue   = new ProductionLine(RectBluePose,   LegoColor.blue,   Direction.up, Direction.down);
        productionRed    = new ProductionLine(RectRedPose,    LegoColor.red,    Direction.up, Direction.down);
        productionGreen  = new ProductionLine(RectGreenPose,  LegoColor.green,  Direction.left, Direction.right);
        alleLinien = new List<ProductionLine> { productionYellow, productionRed, productionBlue, productionGreen };
    }

    // Bestückt alle Linien mit Bricks in Zufallsfarben (bzw. würfelt die Farben der Bricks auf den Linien neu).
    public void SetupProductionLines()
    {
        foreach (ProductionLine line in alleLinien)
            line.CreateBricks();
    }

    // Neue zufällige Bestellung (maximal orderSize je Farbe).
    public void CreateOrder()
    {
        logistics.CreateRandomOrder();
        lastDelivery = null;
    }

    // Nach jeder Bewegung prüft der Roboter alle Bricks auf Kontakt → Andocken passiert automatisch.
    public void RobotBewegen(Direction dir)
    {
        robot.Bewegen(dir);
        KontaktPruefen();
    }

    private void KontaktPruefen()
    {
        foreach (LegoBrick brick in AlleBricks)
            robot.Kontakt(brick);
    }

    // GoToPickUp: Teleport vor den Brick, den der Logistik-Plan vorgibt – auf einer Produktionslinie
    // oder (mit Priorität) auf der Recycling-Linie. Der Roboter schaut dabei auf die Linie.
    // Rückgabe: false, wenn der Plan gerade nichts zum Abholen vorgibt oder die Zielpose blockiert ist.
    public bool GoToLine()
    {
        LogisticsStep schritt = LogisticsStep;
        bool ok = schritt.Art switch
        {
            LogisticsStepKind.abholen when schritt.Line != null && schritt.Brick != null
                => robot.GeheZuLinie(schritt.Line.MittlereBrickMitte(schritt.Brick), GoToAngle(schritt.Line.Color)),
            LogisticsStepKind.recyclingAbholen when schritt.Brick != null && schritt.Brick.RecyclePlatz >= 0
                => robot.GeheZuLinie(RecyclePlatzMitte(schritt.Brick.RecyclePlatz), GoToRecycleAngle),
            _ => false,
        };
        if (ok)
            KontaktPruefen();
        return ok;
    }

    // GoToDelivery: Teleport an die Delivery-Linie (ein gedockter Brick wird mitgenommen).
    public bool GoToDelivery()
    {
        robot.RobotDirection = Direction.stop;
        return robot.SetzePose(GoToDeliveryPose);
    }

    // Blickrichtung des Roboters je Produktionslinie
    private static double GoToAngle(LegoColor line) => line switch
    {
        LegoColor.yellow => GoToYellowAngle,
        LegoColor.green => GoToGreenAngle,
        LegoColor.red => GoToRedAngle,
        _ => GoToBlueAngle,
    };

    // Jede Linie bewegt ihre Bricks selbst, die Factory gibt nur den Takt weiter.
    // Dazu fahren die Bricks auf dem Lieferband zum Kunden bzw. zum Recycling.
    // Ein ausgelieferter Brick wird von der Logistik gezählt und von seiner Linie als Ersatz neu produziert.
    // Auch ein Brick, der auf dem Band in den Greifer läuft, wird angedockt.
    public void LegoBricksBewegen()
    {
        foreach (ProductionLine line in alleLinien)
        {
            line.Bewegen();
            foreach (LegoBrick brick in line.Bricks)
            {
                brick.Bewegen();
                if (brick.Status == BrickStatus.ausgeliefert)
                {
                    lastDelivery = logistics.BrickDelivered(brick.Color);
                    line.Nachproduzieren(brick);
                }
            }
        }
        KontaktPruefen();
    }

    // Abdocken nur auf der Delivery-Linie. Der Brick wird auf das Lieferband gelegt und ist zufällig
    // (50 %) gut (→ Kunde) oder defekt (→ freier Platz auf der Recycling-Linie).
    public UndockResult Undock()
    {
        if (robot.Dockobjekt is not LegoBrick brick)
            return UndockResult.keinBrick;
        if (!robot.AufDeliveryLinie())
            return UndockResult.nichtAufDeliveryLinie;

        bool defekt = naechsterZumRecycling;
        int platz = FreierRecyclePlatz();
        if (defekt && platz < 0)
            return UndockResult.recyclingVoll;

        robot.Undock();
        brick.Location = new Point(brick.X, (int)DeliveryLineY - BrickHeight / 2);
        brick.Angle = 0;
        if (defekt)
            brick.ZumRecycling(platz, RecyclePlatzPose(platz).Location);
        else
            brick.ZumKunden();

        naechsterZumRecycling = random.Next(2) == 0;   // 0 oder 1 → je 50 %
        return defekt ? UndockResult.zumRecycling : UndockResult.zumKunden;
    }

    // Erster Platz der Recycling-Linie, der von keinem Brick belegt ist (-1 = alle 4 belegt).
    private int FreierRecyclePlatz()
    {
        for (int platz = 0; platz < RecyclePlatzMitteX.Length; platz++)
            if (!AlleBricks.Any(b => b.RecyclePlatz == platz))
                return platz;
        return -1;
    }

    // Linke obere Ecke eines Bricks auf dem Recycling-Platz
    private static Pose RecyclePlatzPose(int platz)
    {
        return new Pose(UntenX + RecyclePlatzMitteX[platz] - BrickWidth / 2, RecyclePlatzMitteY - BrickHeight / 2, 0);
    }

    // Mitte eines Recycling-Platzes (Ziel für GoToPickUp)
    private static PointF RecyclePlatzMitte(int platz)
    {
        return new PointF((float)(UntenX + RecyclePlatzMitteX[platz]), (float)RecyclePlatzMitteY);
    }
}
