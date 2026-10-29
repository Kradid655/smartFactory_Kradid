using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace smartFactoryApp.Models
{
    public class Beweger
    {
        // Größe des Roboters in Pixeln (wie das RobotRectangle in MainWindow.axaml)
        public const int RobotWidth = 70;
        public const int RobotHeight = 50;

        // Kontakt besteht, wenn der Mittelpunkt eines Legosteins höchstens so weit vom Greifpunkt entfernt ist
        private const int kontaktAbstand = 10;

        // So weit fährt der Roboter nach dem Abdocken rückwärts, damit er den Stein nicht sofort wieder andockt.
        // Muss in jeder Pose auf mindestens einer Achse > kontaktAbstand ergeben (bei 45° nur ≈ 0,7 * Abstand pro Achse).
        private const int undockRueckweg = 15;

        private double angle;
        private Rectangle borders;
        private int x; // beim Roboter und beim Legostein die linke obere Ecke
        private int y;
        protected Beweger? dockobjekt;    // gedocktes Objekt (z.B. Legostein), null = Roboter ist frei
        private double dockWinkelOffset;  // Winkel des gedockten Objekts relativ zum Roboter (bleibt beim Drehen erhalten)

        // Die Förderbänder als Hindernisse für den Roboter: je Band der Mittelpunkt (X, Y) und der Winkel.
        // Die Größe ist für alle gleich (ProductionLineWidth x ProductionLineHeight).
        // Mittelpunkt wie in MainWindow.axaml: Linienstart (x, y) + (7, 6) + 14 px in Richtung des Bandes,
        // also bei Winkel 0 → (x+21, y+6), bei Winkel 90 → (x+7, y+20).
        private static readonly List<Pose> productionLines = new[]
        {
            ViewParameters.RectYellowPose, ViewParameters.RectBluePose,
            ViewParameters.RectRedPose,    ViewParameters.RectGreenPose,
        }.Select(p => new Pose(
            p.X + 7 + 14 * Math.Cos(p.Angle * Math.PI / 180),
            p.Y + 6 + 14 * Math.Sin(p.Angle * Math.PI / 180),
            p.Angle)).ToList();

        // Die Bilder Oben und Unten als weitere Hindernisse: Mittelpunkt (Winkel 0) und Größe.
        private static readonly (Pose Mitte, SizeF Groesse)[] bildHindernisse =
        {
            (new Pose(ViewParameters.ObenX + ViewParameters.ObenWidth / 2, ViewParameters.ObenY + ViewParameters.ObenHeight / 2, 0),
             new SizeF((float)ViewParameters.ObenWidth, (float)ViewParameters.ObenHeight)),
            (new Pose(ViewParameters.UntenX + ViewParameters.UntenWidth / 2, ViewParameters.UntenY + ViewParameters.UntenHeight / 2, 0),
             new SizeF((float)ViewParameters.UntenWidth, (float)ViewParameters.UntenHeight)),
        };

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
            // 1. Schritt aus Sicht des Bewegers (dx, dy): "up" = vorwärts, "right" = nach rechts usw.
            // Bildschirmkoordinaten: Y wächst nach unten → "up" = dy negativ, "down" = dy positiv.
            // Diagonal: dx und dy ändern sich gleichzeitig um step.
            int dx = 0;
            int dy = 0;
            switch (direction)
            {
                case Parameters.Direction.up:        dy = -Parameters.step; break;
                case Parameters.Direction.down:      dy = Parameters.step; break;
                case Parameters.Direction.left:      dx = -Parameters.step; break;
                case Parameters.Direction.right:     dx = Parameters.step; break;
                case Parameters.Direction.RightDown: dx = Parameters.step;  dy = Parameters.step; break;
                case Parameters.Direction.RightUp:   dx = Parameters.step;  dy = -Parameters.step; break;
                case Parameters.Direction.LeftDown:  dx = -Parameters.step; dy = Parameters.step; break;
                case Parameters.Direction.LeftUp:    dx = -Parameters.step; dy = -Parameters.step; break;
                default:
                    break;
            }

            // 2. Der Roboter fährt relativ zu seiner Blickrichtung: (dx, dy) wird um seinen Winkel gedreht
            //    (positiver Winkel = im Uhrzeigersinn, wie RotateTransform in der View).
            //    Bei 90° wird aus "up" (0, -step) also (step, 0) = nach rechts.
            //    Legosteine fahren weiter absolut (ihr Winkel ist nur die Lage auf dem Förderband).
            double rad = this is Robot ? Angle * Math.PI / 180 : 0;
            Point newLoc = new Point(
                X + (int)Math.Round(dx * Math.Cos(rad) - dy * Math.Sin(rad)),
                Y + (int)Math.Round(dx * Math.Sin(rad) + dy * Math.Cos(rad)));

            // 3. Übernommen wird die neue Position nur, wenn alle Prüfungen bestanden sind.
            bool innerhalbGrenzen = newLoc.X >= Borders.Left && newLoc.X <= Borders.Right
                                 && newLoc.Y >= Borders.Top && newLoc.Y <= Borders.Bottom;
            if (!innerhalbGrenzen || KollidiertMitProduktionslinie(newLoc, Angle) || !DockobjektInnerhalbGrenzen(newLoc, Angle))
                return;

            Location = newLoc;
            DockobjektNachziehen();
        }

        // Dreht um winkel Grad (positiv = im Uhrzeigersinn). Ein gedocktes Objekt dreht sich mit.
        // Auch beim Drehen darf der Roboter nicht in ein Förderband hineinschwenken.
        public void Drehen(double winkel)
        {
            double newAngle = Angle + winkel;
            if (KollidiertMitProduktionslinie(Location, newAngle) || !DockobjektInnerhalbGrenzen(Location, newAngle))
                return;

            Angle = newAngle;
            DockobjektNachziehen();
        }

        // Prüft, ob das gedockte Objekt bei Roboterposition p und Winkel a noch innerhalb seiner Grenzen läge
        private bool DockobjektInnerhalbGrenzen(Point p, double a)
        {
            if (dockobjekt == null)
                return true;
            Point tcp = Greifpunkt(p, a);
            return tcp.X >= dockobjekt.Borders.Left && tcp.X <= dockobjekt.Borders.Right
                && tcp.Y >= dockobjekt.Borders.Top && tcp.Y <= dockobjekt.Borders.Bottom;
        }

        // Setzt das gedockte Objekt mit seinem Mittelpunkt auf den Greifpunkt und dreht es mit dem Roboter mit
        private void DockobjektNachziehen()
        {
            if (dockobjekt == null)
                return;
            Point tcp = Greifpunkt(Location, Angle);
            dockobjekt.Location = new Point(tcp.X - ViewParameters.BrickWidth / 2, tcp.Y - ViewParameters.BrickHeight / 2);
            dockobjekt.Angle = Angle + dockWinkelOffset;
        }

        // Greifpunkt (TCP) des Roboters: Mitte seiner Oberkante.
        // p = linke obere Ecke des Roboters, a = Winkel in Grad. Die View dreht den Roboter um seinen Mittelpunkt,
        // daher wird der Vektor Mittelpunkt → Oberkante (0, -RobotHeight/2) um a gedreht.
        private static Point Greifpunkt(Point p, double a)
        {
            double rad = a * Math.PI / 180;
            double mx = p.X + RobotWidth / 2.0;
            double my = p.Y + RobotHeight / 2.0;
            double tx = mx + RobotHeight / 2.0 * Math.Sin(rad);
            double ty = my - RobotHeight / 2.0 * Math.Cos(rad);
            return new Point((int)Math.Round(tx), (int)Math.Round(ty));
        }

        // Prüft, ob der Roboter an Position p (linke obere Ecke) mit Winkel a eines der Förderbänder
        // oder eines der Bilder Oben/Unten überlappt.
        // Die View dreht den Roboter um seinen Mittelpunkt → sein Rechteck hat den Mittelpunkt p + (RobotWidth/2, RobotHeight/2).
        private bool KollidiertMitProduktionslinie(Point p, double a)
        {
            if (this is not Robot)
                return false;
            Pose robot = new Pose(p.X + RobotWidth / 2.0, p.Y + RobotHeight / 2.0, a);
            SizeF robotGroesse = new SizeF(RobotWidth, RobotHeight);
            SizeF bandGroesse = new SizeF((float)ViewParameters.ProductionLineWidth, (float)ViewParameters.ProductionLineHeight);

            foreach (Pose band in productionLines)
                if (RechteckeUeberlappen(robot, robotGroesse, band, bandGroesse))
                    return true;
            foreach ((Pose mitte, SizeF groesse) in bildHindernisse)
                if (RechteckeUeberlappen(robot, robotGroesse, mitte, groesse))
                    return true;
            return false;
        }

        // Prüft, ob sich zwei (beliebig gedrehte) Rechtecke überlappen. Jedes Rechteck ist gegeben durch
        // Mittelpunkt + Winkel (Pose) und Größe. Verfahren: Trennachsen-Satz (Separating Axis Theorem).
        // Zwei Rechtecke überlappen sich NICHT, wenn es eine Achse gibt, auf der sich ihre "Schatten" (Projektionen)
        // nicht überschneiden. Bei Rechtecken genügt es, die 4 Kantenrichtungen (2 je Rechteck) zu prüfen.
        private static bool RechteckeUeberlappen(Pose r1, SizeF g1, Pose r2, SizeF g2)
        {
            PointF[] ecken1 = Ecken(r1, g1);
            PointF[] ecken2 = Ecken(r2, g2);

            foreach (PointF[] ecken in new[] { ecken1, ecken2 })
            {
                for (int i = 0; i < 2; i++)
                {
                    // Achse = Richtung der Kante i (Kante 0 und Kante 1 stehen senkrecht aufeinander)
                    float ax = ecken[i + 1].X - ecken[i].X;
                    float ay = ecken[i + 1].Y - ecken[i].Y;
                    (float min1, float max1) = Projektion(ecken1, ax, ay);
                    (float min2, float max2) = Projektion(ecken2, ax, ay);
                    if (max1 <= min2 || max2 <= min1)
                        return false; // Trennachse gefunden → keine Überlappung (Berühren ist erlaubt)
                }
            }
            return true;

            // Die 4 Ecken eines um seinen Mittelpunkt gedrehten Rechtecks (im Uhrzeigersinn, beginnend links oben)
            static PointF[] Ecken(Pose m, SizeF g)
            {
                double rad = m.Angle * Math.PI / 180;
                double cos = Math.Cos(rad), sin = Math.Sin(rad);
                double hw = g.Width / 2, hh = g.Height / 2;
                (int sx, int sy)[] vorzeichen = { (-1, -1), (1, -1), (1, 1), (-1, 1) };
                return vorzeichen.Select(v => new PointF(
                    (float)(m.X + v.sx * hw * cos - v.sy * hh * sin),
                    (float)(m.Y + v.sx * hw * sin + v.sy * hh * cos))).ToArray();
            }

            // Kleinster und größter Wert der Ecken auf der Achse (ax, ay) (Skalarprodukt)
            static (float, float) Projektion(PointF[] ecken, float ax, float ay)
            {
                float[] werte = ecken.Select(e => e.X * ax + e.Y * ay).ToArray();
                return (werte.Min(), werte.Max());
            }
        }

        // Kontakt besteht, wenn der Mittelpunkt des Objekts höchstens kontaktAbstand Pixel vom Greifpunkt entfernt ist.
        // Ist der Roboter noch frei, wird ein noch nicht gedockter Legostein automatisch angedockt.
        public bool Kontakt(Beweger objekt)
        {
            Point tcp = Greifpunkt(Location, Angle);
            int mittelX = objekt.X + ViewParameters.BrickWidth / 2;
            int mittelY = objekt.Y + ViewParameters.BrickHeight / 2;
            bool proximity = Math.Abs(tcp.X - mittelX) <= kontaktAbstand && Math.Abs(tcp.Y - mittelY) <= kontaktAbstand;

            if (proximity && dockobjekt == null && objekt is LegoBrick aBrick && !aBrick.Gedockt)
            {
                aBrick.Gedockt = true;
                dockobjekt = objekt;
                dockWinkelOffset = objekt.Angle - Angle;
                DockobjektNachziehen();
            }
            return proximity;
        }

        // Gibt das gedockte Objekt wieder frei; es bleibt an seiner aktuellen Position liegen.
        // Danach fährt der Roboter undockRueckweg Pixel rückwärts (entgegen seiner Blickrichtung),
        // sofern er dabei innerhalb seiner Grenzen bleibt und nicht mit der Produktionslinie kollidiert.
        public bool Undock()
        {
            if (dockobjekt == null)
                return false;
            if (dockobjekt is LegoBrick aBrick)
                aBrick.Gedockt = false;
            dockobjekt = null;

            // Blickrichtung bei Winkel a ist (sin a, -cos a) → rückwärts ist (-sin a, cos a)
            double rad = Angle * Math.PI / 180;
            Point newLoc = new Point(X - (int)Math.Round(undockRueckweg * Math.Sin(rad)),
                                     Y + (int)Math.Round(undockRueckweg * Math.Cos(rad)));
            if (Borders.Contains(newLoc) && !KollidiertMitProduktionslinie(newLoc, Angle))
                Location = newLoc;
            return true;
        }
    }
}
