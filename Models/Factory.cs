using System.Collections.Generic;
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

    public double RobotX => robot.Location.X;
    public double RobotY => robot.Location.Y;
    // Setter wird für ResetRotation gebraucht (Winkel zurück auf 0).
    public double RobotAngle { get => robot.Angle; set => robot.Angle = value; }

    public List<LegoBrick> YellowBricks => productionYellow.Bricks;
    public List<LegoBrick> RedBricks => productionRed.Bricks;
    public List<LegoBrick> BlueBricks => productionBlue.Bricks;
    public List<LegoBrick> GreenBricks => productionGreen.Bricks;

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
        productionYellow = new ProductionLine(RectYellowPose, LegoColor.yellow, Direction.LeftUp, Direction.RightDown);
        productionBlue   = new ProductionLine(RectBluePose,   LegoColor.blue,   Direction.LeftUp, Direction.RightDown);
        productionRed    = new ProductionLine(RectRedPose,    LegoColor.red,    Direction.RightUp, Direction.LeftDown);
        productionGreen  = new ProductionLine(RectGreenPose,  LegoColor.green,  Direction.RightUp, Direction.LeftDown);
    }

    public void RobotBewegen(Direction dir)
    {
        robot.Bewegen(dir);
    }

    // Jede Linie bewegt ihre Bricks selbst, die Factory gibt nur den Takt weiter.
    public void LegoBricksBewegen()
    {
        productionYellow.Bewegen();
        productionRed.Bewegen();
        productionBlue.Bewegen();
        productionGreen.Bewegen();
    }
}
