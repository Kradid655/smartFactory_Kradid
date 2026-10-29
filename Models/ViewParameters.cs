using System.Drawing;

namespace smartFactoryApp.Models;


    public static class ViewParameters
    {
        // Startpose des Roboters (linke obere Ecke). Liegt mittig unter dem Bild "Unten" (endet bei y = 336)
        // und über dem grünen Förderband (beginnt bei y ≈ 449), damit er in keinem Hindernis startet.
        public static Pose RobotPose = new Pose(265, 360, 0);
        public static Rectangle Borders = new Rectangle(37, 37, 452, 480);       // Grenzen für den Roboter (70x50)
        public static Rectangle BrickBorders = new Rectangle(37, 37, 510, 510);  // Grenzen für die Bricks: graue Fabrikfläche

         // Startposition des Roboters (linke obere Ecke, muss innerhalb von Borders liegen!)
        public static readonly Point RobotPosition = new Point(265, 360);

        // Bilder Oben.png und Unten.png (linke obere Ecke + Größe in Pixeln), zugleich Hindernisse für den Roboter.
        // Die graue Fabrikfläche reicht von 38 bis 563 → Mitte bei (300, 300).
        // Oben: waagerecht mittig am oberen Rand der Fläche.
        public const double ObenWidth = 228;
        public const double ObenHeight = 76;
        public const double ObenX = 300 - ObenWidth / 2;   // 186
        public const double ObenY = 38;
        // Unten: in der Mitte der Fläche.
        public const double UntenWidth = 193;
        public const double UntenHeight = 72;
        public const double UntenX = 300 - UntenWidth / 2;   // 203.5
        public const double UntenY = 300 - UntenHeight / 2;  // 264

        // Größe eines Förderbands in Pixeln (für das Foerderband.png in der View).
        // Die Bricks belegen inkl. Bewegungsweg (10 Schritte * 3 px = 30 px) ca. 104 px;
        // das Band ist länger, damit sie nicht in den runden Enden liegen.
        // Die Breite quer zum Band ist so schmal, dass der Greifpunkt am Bandrand nah genug (≤ 10 px) an die
        // Brick-Mitte kommt: halbe Breite (7) + bis zu 3 px Lücke (Schrittweite) ≤ 10 px.
        public const double ProductionLineWidth = 160;
        public const double ProductionLineHeight = 14;

        // Startpunkt + Winkel jedes Förderbands.
        // Winkel 0  = Band verläuft waagerecht, passend zu left/right.
        // Winkel 90 = Band verläuft senkrecht, passend zu up/down.
        // PLATZHALTER-Koordinaten: an factory_plan.png anpassen!
        public static Pose RectYellowPose = new Pose(125, 450, 0);
        public static Pose RectBluePose   = new Pose(475, 200, 90);
        public static Pose RectRedPose    = new Pose(125, 150, 90);
        public static Pose RectGreenPose  = new Pose(350, 450, 0);

        // Größe eines Legosteins in Pixeln (wie die Brick-Rectangles in MainWindow.axaml)
        public const int BrickWidth = 16;
        public const int BrickHeight = 12;
    }
