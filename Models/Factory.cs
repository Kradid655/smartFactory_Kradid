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
        foreach (LegoBrick brick in YellowBricks.Concat(RedBricks).Concat(BlueBricks).Concat(GreenBricks))
            robot.Kontakt(brick);
    }

    // Jede Linie bewegt ihre Bricks selbst, die Factory gibt nur den Takt weiter.
    // Auch ein Brick, der auf dem Band in den Greifer läuft, wird angedockt.
    public void LegoBricksBewegen()
    {
        productionYellow.Bewegen();
        productionRed.Bewegen();
        productionBlue.Bewegen();
        productionGreen.Bewegen();
        foreach (LegoBrick brick in YellowBricks.Concat(RedBricks).Concat(BlueBricks).Concat(GreenBricks))
            robot.Kontakt(brick);
    }

    // Der Roboter lässt den gedockten Brick an seiner aktuellen Position los.
    public bool Undock()
    {
        return robot.Undock();
    }
}
