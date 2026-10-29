using System;
using System.Collections.Generic;
using System.Drawing;
using static smartFactoryApp.Models.Parameters;
using static smartFactoryApp.Models.ViewParameters;

namespace smartFactoryApp.Models
{
    public class LegoBrick : Beweger
    {
        private BrickStatus status = BrickStatus.aufLinie;
        private int recyclePlatz = -1;                     // belegter Platz auf der Recycling-Linie, -1 = keiner
        private readonly Queue<Point> weg = new Queue<Point>(); // Wegpunkte (linke obere Ecke) beim Transport
        private Point recycleZiel;                         // Platz auf der Recycling-Linie, dorthin wird teleportiert

        public BrickStatus Status { get => status; set => status = value; }
        public int RecyclePlatz => recyclePlatz;
        // Angedockt werden darf nur von einer Produktionslinie oder der Recycling-Linie.
        public bool Andockbar => status == BrickStatus.aufLinie || status == BrickStatus.imRecycling;

        private LegoColor color;
        private LegoSize size;
        public LegoColor Color { get { return color; } set { color = value; } }
        public LegoSize Size { get { return size; } set { size = value; } }

        // Bricks bewegen sich nur innerhalb der Fabrikfläche (BrickBorders), nicht in den Roboter-Grenzen.
        public LegoBrick(Pose pose, LegoColor color, LegoSize size) : base(pose, BrickBorders)
        {
            Color = color;
            Size = size;
        }

        // Nur Bricks auf ihrer Produktionslinie laufen mit dem Förderband hin und her.
        public override void Bewegen(Parameters.Direction dir)
        {
            if (Status == BrickStatus.aufLinie)
                base.Bewegen(dir);
        }

        // Der Roboter nimmt den Brick auf; ein Platz auf der Recycling-Linie wird dadurch wieder frei.
        public void Andocken()
        {
            status = BrickStatus.gedockt;
            recyclePlatz = -1;
        }

        // Gut: auf dem Lieferband nach rechts bis zur Spitze des Pfeils "To Customers".
        public void ZumKunden()
        {
            status = BrickStatus.zumKunden;
            weg.Clear();
            weg.Enqueue(new Point((int)(ToCustomerLineX + ToCustomerLineWidth) - BrickWidth, Y));
        }

        // Defekt: nach links bis zur Spitze des Pfeils "For Recycling", von dort wird er
        // auf den Platz der Recycling-Linie teleportiert (ziel = linke obere Ecke).
        public void ZumRecycling(int platz, Point ziel)
        {
            status = BrickStatus.zumRecycling;
            recyclePlatz = platz;
            recycleZiel = ziel;
            weg.Clear();
            weg.Enqueue(new Point((int)ToRecycleLineX, Y));
        }

        // Transport bei jedem Timer-Tick: ein Schritt zum nächsten Wegpunkt.
        // Am Ende ist der Brick beim Kunden (ausgeliefert) bzw. auf der Recycling-Linie (imRecycling).
        public void Bewegen()
        {
            if (status != BrickStatus.zumKunden && status != BrickStatus.zumRecycling)
                return;
            if (weg.Count > 0 && SchrittZu(weg.Peek()))
                weg.Dequeue();
            if (weg.Count > 0)
                return;
            if (status == BrickStatus.zumKunden)
            {
                status = BrickStatus.ausgeliefert;
            }
            else
            {
                Location = recycleZiel;
                status = BrickStatus.imRecycling;
            }
        }

        // Je Achse höchstens step Pixel in Richtung Ziel; true, wenn das Ziel erreicht ist.
        private bool SchrittZu(Point ziel)
        {
            int dx = Math.Clamp(ziel.X - X, -step, step);
            int dy = Math.Clamp(ziel.Y - Y, -step, step);
            Location = new Point(X + dx, Y + dy);
            return Location == ziel;
        }

        public override string ToString()
        {
            return $"LegoBrick {Color} ({Size}) bei ({X}, {Y}), Winkel {Angle}°, {Status}";
        }
    }
}
