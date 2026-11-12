using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using static smartFactoryApp.Models.Parameters;
using static smartFactoryApp.Models.ViewParameters;

namespace smartFactoryApp.Models;

// Eine Produktionslinie ist ein Förderband, auf dem mehrere LegoBricks liegen.
// Beim Start ist sie leer; erst CreateBricks (über Factory.SetupProductionLines) legt Bricks auf.
// Jeder Brick bekommt zufällig eine der 4 Farben (je 25 %), unabhängig von der Linie.
// Die Linie bewegt ihre Bricks hin und her (Bewegen).
public class ProductionLine
{
    // Wie viele Timer-Ticks die Bricks in eine Richtung laufen, bevor sie umkehren.
    private const int stepsPerDirection = 10;

    // Abstand in Pixeln zwischen zwei Bricks auf der Linie.
    private const int brickDistance = 20;

    private static readonly LegoColor[] alleFarben = Enum.GetValues<LegoColor>();
    private static readonly Random random = new Random();

    private double angle;                                 // Drehwinkel der Linie, wird an jeden Brick weitergegeben
    private List<LegoBrick> bricks = new List<LegoBrick>(); // alle Bricks dieser Linie (am Anfang leer)
    private LegoColor color;                              // Name/Kennfarbe der Linie (nicht die Farbe der Bricks!)
    private Direction outwardDirection;                   // Richtung in der ersten Hälfte des Zyklus (hin)
    private Direction backDirection;                      // Gegenrichtung in der zweiten Hälfte (zurück)
    private int tickCount;                                // zählt die Timer-Ticks, um die Bewegungsrichtung zu bestimmen
    private int versatz;                                  // wie viele Schritte die Bricks gerade "hin" verschoben sind (0..10)
    private int x;                                        // Startposition der Linie (X)
    private int y;                                        // Startposition der Linie (Y)

    public double Angle => angle;
    public List<LegoBrick> Bricks => bricks;
    public LegoColor Color => color;
    public Point Location => new Point(x, y);
    public Pose Pose => new Pose(x, y, angle);

    public ProductionLine(Pose pose, LegoColor color, Direction outwardDirection, Direction backDirection)
    {
        x = (int)pose.X;
        y = (int)pose.Y;
        angle = pose.Angle;
        this.color = color;
        this.outwardDirection = outwardDirection;
        this.backDirection = backDirection;
        // Keine Bricks: die Linie wird erst über CreateBricks bestückt.
    }

    public ProductionLine(Pose pose, LegoColor color) : this(pose, color, Direction.right, Direction.left)
    {
    }

    public ProductionLine(int x, int y, LegoColor color) : this(new Pose(x, y, 0), color)
    {
    }

    // Beim ersten Aufruf: bricksPerLine neue Bricks mit Zufallsfarbe.
    // Bei jedem weiteren Aufruf: alle Bricks, die noch auf der Linie liegen, bekommen eine neue Zufallsfarbe
    // (Bricks am Roboter, auf dem Lieferband oder im Recycling bleiben, wie sie sind).
    public void CreateBricks()
    {
        if (bricks.Count == 0)
        {
            for (int i = 0; i < bricksPerLine; i++)
            {
                LegoBrick brick = new LegoBrick(StartPose(i), ZufallsFarbe(), LegoSize.small);
                bricks.Add(brick);
                AufVersatzSchieben(brick);   // läuft das Band schon, im Gleichtakt mit ihm starten
            }
            return;
        }
        foreach (LegoBrick brick in bricks)
            if (brick.Status == BrickStatus.aufLinie)
                brick.Color = ZufallsFarbe();
    }

    // Jede Farbe mit 25 % Wahrscheinlichkeit
    private static LegoColor ZufallsFarbe() => alleFarben[random.Next(alleFarben.Length)];

    // Startplatz von Brick i: i * 20 px vom Linienanfang entfernt, waagerecht (left) oder senkrecht (up).
    private Pose StartPose(int i)
    {
        if (outwardDirection == Direction.left)
            return new Pose(x + i * brickDistance, y, angle);
        return new Pose(x, y + i * brickDistance, angle);
    }

    // Verschiebt einen Brick, der auf seinem Startplatz liegt, um den aktuellen Versatz des Bandes.
    private void AufVersatzSchieben(LegoBrick brick)
    {
        for (int i = 0; i < versatz; i++)
            brick.Bewegen(outwardDirection);
    }

    // Ersatz für einen ausgelieferten Brick: er erscheint wieder auf seinem Platz der Linie,
    // mit einer neuen Zufallsfarbe, im Gleichtakt mit den anderen.
    public void Nachproduzieren(LegoBrick brick)
    {
        Pose start = StartPose(bricks.IndexOf(brick));
        brick.Location = start.Location;
        brick.Angle = start.Angle;
        brick.Color = ZufallsFarbe();
        brick.Status = BrickStatus.aufLinie;
        AufVersatzSchieben(brick);
    }

    // Mitte des Bereichs, in dem ein Brick hin und her läuft (Startplatz + halber Fahrweg).
    // Dorthin zielt der Greifpunkt des Roboters bei GoToPickUp.
    public PointF MittlereBrickMitte(LegoBrick brick)
    {
        Pose start = StartPose(bricks.IndexOf(brick));
        double halberWeg = stepsPerDirection * step / 2.0;   // 15 px
        (int rx, int ry) = outwardDirection switch
        {
            Direction.left => (-1, 0),
            Direction.right => (1, 0),
            Direction.up => (0, -1),
            Direction.down => (0, 1),
            _ => (0, 0),
        };
        return new PointF((float)(start.X + BrickWidth / 2.0 + rx * halberWeg),
                          (float)(start.Y + BrickHeight / 2.0 + ry * halberWeg));
    }

    // Wird bei jedem Timer-Tick aufgerufen.
    public void Bewegen()
    {
        bool outward = tickCount < stepsPerDirection;
        tickCount = (tickCount + 1) % (2 * stepsPerDirection);
        versatz += outward ? 1 : -1;

        foreach (LegoBrick brick in bricks)
        {
            if (outward)
                brick.Bewegen(outwardDirection);
            else
                brick.Bewegen(backDirection);
        }
    }
}
