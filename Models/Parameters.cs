namespace smartFactoryApp.Models;
public class Parameters
{
    public enum Direction
    {
        stop = 0,
        up = 1,
        down = 2,
        left = 3,
        right = 4,
        RightUp=5,
        LeftUp=6,
        RightDown=7,
        LeftDown=8,
        rotateLeft=9,   // nur Robot: Winkel - rotationStep
        rotateRight=10  // nur Robot: Winkel + rotationStep
    }
    public enum LegoColor
    {
        yellow = 0,
        blue = 1,
        red = 2,
        green = 3,
    }
    public enum LegoSize
    {
        large = 0,
        small = 1,
    }
    public const int bricksPerLine = 4;
    public const double rotationStep = 10;
    public const int timerInterval = 50; //Timer-Intervall in Millisekunden
    public const int step = 3;
}