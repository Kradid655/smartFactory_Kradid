using System.Drawing;

namespace smartFactoryApp.Models
{
    public class Beweger
    {
        private double angle;
        private Rectangle borders;
        private int x;
        private int y;

        public double Angle { get { return angle; } set { angle = value; } }
        public Rectangle Borders { get => borders; set => borders = value; }
        public int X { get { return x; } set { x = value; } }
        public int Y { get { return y; } set { y = value; } }

        // x und y zusammengefasst als Punkt (für alle, die mit Location arbeiten).
        public Point Location
        {
            get { return new Point(x, y); }
            set { x = value.X; y = value.Y; }
        }

        // Haupt-Konstruktor: alle anderen Überladungen rufen ihn über ": this(...)" auf.
        public Beweger(Pose pose, Rectangle borders)
        {
            Location = pose.Location;
            Angle = pose.Angle;
            Borders = borders;
        }
        public Beweger(Point location, Rectangle borders) : this(new Pose(location.X, location.Y, 0), borders)
        {
        }
        // Ohne eigene Grenzen → Standardgrenzen aus ViewParameters.
        public Beweger(Point location) : this(location, ViewParameters.Borders)
        {
        }
        public Beweger(Pose pose) : this(pose, ViewParameters.Borders)
        {
        }

        public virtual void Bewegen(Parameters.Direction direction)
        {
            switch (direction)
            {
                case Parameters.Direction.up:
                    if (Y - Parameters.step >= Borders.Top)
                        Y -= Parameters.step;
                    break;
                case Parameters.Direction.down:
                    if (Y + Parameters.step <= Borders.Bottom)
                        Y += Parameters.step;
                    break;
                case Parameters.Direction.left:
                    if (X - Parameters.step >= Borders.Left)
                        X -= Parameters.step;
                    break;
                case Parameters.Direction.right:
                    if (X + Parameters.step <= Borders.Right)
                        X += Parameters.step;
                    break;
                // Diagonal: X und Y ändern sich gleichzeitig um step.
                // Bildschirmkoordinaten: Y wächst nach unten → "Up" = Y - step, "Down" = Y + step.
                // Bewegt wird nur, wenn beide neuen Koordinaten innerhalb der Grenzen liegen.
                case Parameters.Direction.RightDown:
                    if (X + Parameters.step <= Borders.Right && Y + Parameters.step <= Borders.Bottom)
                    { X += Parameters.step; Y += Parameters.step; }
                    break;
                case Parameters.Direction.RightUp:
                    if (X + Parameters.step <= Borders.Right && Y - Parameters.step >= Borders.Top)
                    { X += Parameters.step; Y -= Parameters.step; }
                    break;
                case Parameters.Direction.LeftDown:
                    if (X - Parameters.step >= Borders.Left && Y + Parameters.step <= Borders.Bottom)
                    { X -= Parameters.step; Y += Parameters.step; }
                    break;
                case Parameters.Direction.LeftUp:
                    if (X - Parameters.step >= Borders.Left && Y - Parameters.step >= Borders.Top)
                    { X -= Parameters.step; Y -= Parameters.step; }
                    break;
                default:
                    break;
            }
        }
    }
}
