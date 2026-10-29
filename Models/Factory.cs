using System;
using System.Collections.Generic;
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

    // 50 % Qualität: ob der nächste abgedockte Brick defekt ist (→ Recycling), wird zufällig ausgewürfelt.
    private static readonly Random zufall = new Random();
    private bool naechsterZumRecycling = zufall.Next(2) == 0;
    // Ausgelieferte Bricks je Farbe
    private readonly Dictionary<LegoColor, int> delivered = new Dictionary<LegoColor, int>();

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
    public IEnumerable<LegoBrick> AlleBricks => YellowBricks.Concat(RedBricks).Concat(BlueBricks).Concat(GreenBricks);

    public int Delivered(LegoColor color) => delivered.GetValueOrDefault(color);

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

        // Jede Linie bekommt ihre Pose aus ViewParameters und ihre beiden Bewegungsrichtungen
        // (die Paare von vorher aus LegoBricksBewegen).
        productionYellow = new ProductionLine(RectYellowPose, LegoColor.yellow, Direction.left, Direction.right);
        productionBlue   = new ProductionLine(RectBluePose,   LegoColor.blue,   Direction.up, Direction.down);
        productionRed    = new ProductionLine(RectRedPose,    LegoColor.red,    Direction.up, Direction.down);
        productionGreen  = new ProductionLine(RectGreenPose,  LegoColor.green,  Direction.left, Direction.right);
    }

    // Nach jeder Bewegung prüft der Roboter alle Bricks auf Kontakt → Andocken passiert automatisch.
    public void RobotBewegen(Direction dir)
    {
        robot.Bewegen(dir);
        foreach (LegoBrick brick in AlleBricks)
            robot.Kontakt(brick);
    }

    // Jede Linie bewegt ihre Bricks selbst, die Factory gibt nur den Takt weiter.
    // Dazu fahren die Bricks auf dem Lieferband zum Kunden bzw. zum Recycling.
    // Ein ausgelieferter Brick wird gezählt und von seiner Linie als Ersatz neu produziert.
    // Auch ein Brick, der auf dem Band in den Greifer läuft, wird angedockt.
    public void LegoBricksBewegen()
    {
        foreach (ProductionLine line in new[] { productionYellow, productionRed, productionBlue, productionGreen })
        {
            line.Bewegen();
            foreach (LegoBrick brick in line.Bricks)
            {
                brick.Bewegen();
                if (brick.Status == BrickStatus.ausgeliefert)
                {
                    delivered[brick.Color] = Delivered(brick.Color) + 1;
                    line.Nachproduzieren(brick);
                }
            }
        }
        foreach (LegoBrick brick in AlleBricks)
            robot.Kontakt(brick);
    }

    // Abdocken nur auf der Delivery-Linie. Der Brick wird auf das Lieferband gelegt und ist zufällig
    // (50/50) gut (→ Kunde) oder defekt (→ freier Platz auf der Recycling-Linie).
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
        brick.Location = new System.Drawing.Point(brick.X, (int)DeliveryLineY - BrickHeight / 2);
        brick.Angle = 0;
        if (defekt)
            brick.ZumRecycling(platz, RecyclePlatzPose(platz).Location);
        else
            brick.ZumKunden();

        naechsterZumRecycling = zufall.Next(2) == 0;   // 0 oder 1 → je 50 %
        return defekt ? UndockResult.zumRecycling : UndockResult.zumKunden;
    }

    // Erster Platz der Recycling-Linie, der von keinem Brick belegt ist (-1 = alle belegt).
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
}
