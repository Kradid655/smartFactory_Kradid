using System.Drawing;
using System.Collections.Generic;
using static smartFactoryApp.Models.Parameters;

namespace smartFactoryApp.Models;

// Eine Produktionslinie ist ein Förderband, auf dem mehrere LegoBricks einer Farbe liegen.
// Die Linie erzeugt ihre Bricks selbst (CreateBricks) und bewegt sie hin und her (Bewegen).
public class ProductionLine
{
    // Wie viele Timer-Ticks die Bricks in eine Richtung laufen, bevor sie umkehren.
    // const, weil sich der Wert zur Laufzeit nie ändert (im Klassendiagramm als Konstante markiert).
    private const int stepsPerDirection = 10;

    // Abstand in Pixeln zwischen zwei Bricks auf der Linie (aus dem alten CreateBricks: -170, -150, -130, -110 → 20 px).
    private const int brickDistance = 20;

    private double angle;                                 // Drehwinkel der Linie, wird an jeden Brick weitergegeben
    private List<LegoBrick> bricks = new List<LegoBrick>(); // alle Bricks dieser Linie
    private LegoColor color;                              // Farbe aller Bricks dieser Linie
    private Direction outwardDirection;                   // Richtung in der ersten Hälfte des Zyklus (hin)
    private Direction backDirection;                      // Gegenrichtung in der zweiten Hälfte (zurück)
    private int tickCount;                                // zählt die Timer-Ticks, um die Bewegungsrichtung zu bestimmen
    private int versatz;                                  // wie viele Schritte die Bricks gerade "hin" verschoben sind (0..10)
    private int x;                                        // Startposition der Linie (X)
    private int y;                                        // Startposition der Linie (Y)

    // Eigenschaften: geben die privaten Felder nach außen (Factory/ViewModel) nur lesend frei.
    public double Angle => angle;
    public List<LegoBrick> Bricks => bricks;
    public LegoColor Color => color;
    public Point Location => new Point(x, y);             // x und y zusammengefasst als Punkt, wie bei Beweger.Location

    // Haupt-Konstruktor: Position/Winkel als Pose, dazu Farbe und die beiden Bewegungsrichtungen.
    // Jede Linie weiß selbst, wie sich ihre Bricks bewegen → Factory muss das nicht mehr wissen.
    public ProductionLine(Pose pose, LegoColor color, Direction outwardDirection, Direction backDirection)
    {
        x = (int)pose.X;      // Pose speichert double, die Linie arbeitet mit ganzen Pixeln
        y = (int)pose.Y;
        angle = pose.Angle;
        this.color = color;
        this.outwardDirection = outwardDirection;
        this.backDirection = backDirection;
        CreateBricks();       // die Linie ist erst fertig, wenn ihre Bricks existieren
    }

    // Überladung 1: ohne Richtungen → Standard ist waagerecht hin und her (rechts/links).
    // ": this(...)" ruft den Haupt-Konstruktor auf, damit die Logik nur an einer Stelle steht.
    public ProductionLine(Pose pose, LegoColor color) : this(pose, color, Direction.right, Direction.left)
    {
    }

    // Überladung 2: Position direkt als Zahlen, Winkel 0 = Linie liegt waagerecht.
    public ProductionLine(int x, int y, LegoColor color) : this(new Pose(x, y, 0), color)
    {
    }

    // Erzeugt bricksPerLine (= 4, aus Parameters) Bricks entlang der Linie.
    private void CreateBricks()
    {
        for (int i = 0; i < bricksPerLine; i++)
            bricks.Add(new LegoBrick(StartPose(i), color, LegoSize.small));
    }

    // Startplatz von Brick i: i * 20 px vom Linienanfang entfernt, waagerecht (left) oder senkrecht (up).
    private Pose StartPose(int i)
    {
        if (outwardDirection == Direction.left)
            return new Pose(x + i * brickDistance, y, angle);
        return new Pose(x, y + i * brickDistance, angle);
    }

    // Ersatz für einen ausgelieferten Brick: er erscheint wieder auf seinem Platz der Linie
    // und wird um den aktuellen Versatz verschoben, damit er im Gleichtakt mit den anderen läuft.
    public void Nachproduzieren(LegoBrick brick)
    {
        Pose start = StartPose(bricks.IndexOf(brick));
        brick.Location = start.Location;
        brick.Angle = start.Angle;
        brick.Status = BrickStatus.aufLinie;
        for (int i = 0; i < versatz; i++)
            brick.Bewegen(outwardDirection);
    }

    // Wird bei jedem Timer-Tick aufgerufen (übernommen aus Factory.LegoBricksBewegen).
    public void Bewegen()
    {
        // In den ersten stepsPerDirection Ticks "hin", danach genauso lange "zurück".
        bool outward = tickCount < stepsPerDirection;

        // Zähler läuft 0..19 und beginnt dann wieder bei 0 → die Bewegung wiederholt sich endlos.
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
