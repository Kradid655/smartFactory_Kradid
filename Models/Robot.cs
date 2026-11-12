using System;
using System.Drawing;
namespace smartFactoryApp.Models;

public class Robot : Beweger
{
    // So weit (in Pixeln) steht der Greifpunkt nach GoToPickUp vor der Brick-Mitte.
    // Größer als der Kontaktabstand (10), damit nicht sofort irgendein Brick angedockt wird:
    // der Roboter fährt danach selbst ein paar Schritte vorwärts ("Up") und greift den gewünschten Brick.
    private const int abstandZurLinie = 22;

    // Richtung für die kontinuierliche Bewegung (Move-Modus): wird bei jedem Timer-Tick gefahren.
    private Parameters.Direction robotDirection = Parameters.Direction.stop;

    public Parameters.Direction RobotDirection { get => robotDirection; set => robotDirection = value; }

    public Robot(Point location, Rectangle borders) : base(location, borders)
    {
    }

    public Robot(Pose pose) : base(pose)
    {
    }

    // Der Roboter kann sich zusätzlich drehen; alle anderen Richtungen erledigt Beweger.
    public override void Bewegen(Parameters.Direction direction)
    {
        switch (direction)
        {
            case Parameters.Direction.rotateLeft:
                Drehen(-Parameters.rotationStep);
                break;
            case Parameters.Direction.rotateRight:
                Drehen(Parameters.rotationStep);
                break;
            default:
                base.Bewegen(direction);
                break;
        }
    }

    // Teleportiert den Roboter vor eine Linie: er schaut mit dem Winkel winkel auf das Ziel
    // (Brick-Mitte bzw. Recycling-Platz), sein Greifpunkt steht abstandZurLinie Pixel davor.
    // Rückgabe: false, wenn die Zielpose blockiert ist (dann bleibt der Roboter, wo er ist).
    public bool GeheZuLinie(PointF ziel, double winkel)
    {
        double rad = winkel * Math.PI / 180;
        double fx = Math.Sin(rad), fy = -Math.Cos(rad);              // Blickrichtung des Roboters
        double abstand = abstandZurLinie + RobotHeight / 2.0;        // Ziel → Robotermitte
        double mx = ziel.X - abstand * fx;
        double my = ziel.Y - abstand * fy;
        robotDirection = Parameters.Direction.stop;
        return SetzePose(new Pose(Math.Round(mx - RobotWidth / 2.0), Math.Round(my - RobotHeight / 2.0), winkel));
    }
}
