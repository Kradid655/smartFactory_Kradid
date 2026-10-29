using System.Drawing;
namespace smartFactoryApp.Models;

public class Robot : Beweger
{
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
}
