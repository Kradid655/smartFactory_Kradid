using System;
using System.Drawing;
using static smartFactory.Models.Parameters;

namespace smartFactory.Models;

internal class Robot
{
    public const int Size = 50;   // Kantenlänge des Roboters
    public const int Step = 10;   // Pixel pro Klick

    public Point Location { get; private set; }
    public Rectangle Borders { get; }
    public int Angle { get; private set; }   // 0, 90, 180, 270

    public Robot(Point location, Rectangle borders)
    {
        Location = location;
        Borders = borders;
    }

    public void Move(Direction direction)
    {
        int dx = 0, dy = 0;

        switch (direction)
        {
            case Direction.Up:        dy = -1; break;
            case Direction.Down:      dy =  1; break;
            case Direction.Left:      dx = -1; break;
            case Direction.Right:     dx =  1; break;
            case Direction.UpLeft:    dx = -1; dy = -1; break;
            case Direction.UpRight:   dx =  1; dy = -1; break;
            case Direction.DownLeft:  dx = -1; dy =  1; break;
            case Direction.DownRight: dx =  1; dy =  1; break;
            case Direction.Stop:      return;
        }

        // Im Spielfeld bleiben
        int x = Math.Clamp(Location.X + dx * Step, Borders.Left, Borders.Right - Size);
        int y = Math.Clamp(Location.Y + dy * Step, Borders.Top,  Borders.Bottom - Size);
        Location = new Point(x, y);
    }

    public void Rotate(bool right)
    {
        Angle = (Angle + (right ? 90 : -90) + 360) % 360;
    }
}