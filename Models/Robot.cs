using System.Drawing;
namespace smartFactoryApp.Models;

public class Robot : Beweger
{
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
                Angle -= Parameters.rotationStep;
                break;
            case Parameters.Direction.rotateRight:
                Angle += Parameters.rotationStep;
                break;
            default:
                base.Bewegen(direction);
                break;
        }
    }
}
