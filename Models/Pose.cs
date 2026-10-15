using System.Drawing;

namespace smartFactoryApp.Models;

public struct Pose
{
    public double X { get; }
    public double Y { get; }
    public double Angle { get; }
    public Point Location => new Point((int)X, (int)Y);

    public Pose(double x, double y, double angle)
    {
        X = x;
        Y = y;
        Angle = angle;
    }
}
