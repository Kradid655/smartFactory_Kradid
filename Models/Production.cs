using System;
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
        // Winkel von Grad in Bogenmaß umrechnen, weil Math.Cos/Math.Sin Bogenmaß erwarten.
        double radians = angle * Math.PI / 180;

        for (int i = 0; i < bricksPerLine; i++)
        {
            double bx=0;
            double by =0;
            // Brick i liegt i * 20 px vom Linienanfang entfernt, in Richtung des Linienwinkels:
            // cos liefert den X-Anteil, sin den Y-Anteil. Bei Winkel 0 → nur X (waagerecht),
            // bei 45 → X und Y gleich viel (schräg "\"), bei -45 → Y negativ (schräg "/").
            if(outwardDirection==Direction.left){
            bx = x + i * brickDistance;
            by = y;
            }
             else
            {
            bx = x;
            by = y+ i * brickDistance;
            }
            
            bricks.Add(new LegoBrick(new Pose(bx, by, angle), color, LegoSize.small));
        }
    }

    // Wird bei jedem Timer-Tick aufgerufen (übernommen aus Factory.LegoBricksBewegen).
    public void Bewegen()
    {
        // In den ersten stepsPerDirection Ticks "hin", danach genauso lange "zurück".
        bool outward = tickCount < stepsPerDirection;

        // Zähler läuft 0..19 und beginnt dann wieder bei 0 → die Bewegung wiederholt sich endlos.
        tickCount = (tickCount + 1) % (2 * stepsPerDirection);

        foreach (LegoBrick brick in bricks)
        {
            if (outward)
                brick.Bewegen(outwardDirection);
            else
                brick.Bewegen(backDirection);
        }
    }
}
