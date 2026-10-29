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

        // Delivery-Linie = Bild Oben. Die abgedockten Bricks liegen im weißen Streifen unter den Beschriftungen;
        // DeliveryLineY ist die Mitte dieses Streifens (Bild-Zeilen 43..70).
        public const double DeliveryLineY = ObenY + 57;   // 95

        // Pfeile links ("For Recycling") und rechts ("To Customers") neben Oben, mittig auf Höhe des Lieferbands.
        public const double ToRecycleLineWidth = 126;
        public const double ToRecycleLineHeight = 28;
        public const double ToRecycleLineX = ObenX - ToRecycleLineWidth;               // 60
        public const double ToRecycleLineY = DeliveryLineY - ToRecycleLineHeight / 2;  // 81
        public const double ToCustomerLineWidth = 126;
        public const double ToCustomerLineHeight = 28;
        public const double ToCustomerLineX = ObenX + ObenWidth;                         // 414
        public const double ToCustomerLineY = DeliveryLineY - ToCustomerLineHeight / 2;  // 81

        // Recycling-Linie = das farbige Band im Bild Unten (ohne weißen Rand, Bild-Spalten 24..182, Zeilen 20..54).
        // Nur das Band ist Hindernis, damit der Greifpunkt von unten nah genug an die Bricks herankommt.
        public const double RecycleLineX = UntenX + 24;   // 227.5
        public const double RecycleLineY = UntenY + 20;   // 284
        public const double RecycleLineWidth = 158;
        public const double RecycleLineHeight = 34;
        // Mitte der 4 Recycling-Plätze (gelb, rot, grün, blau) in X, relativ zu UntenX.
        public static readonly double[] RecyclePlatzMitteX = { 44, 84, 121, 158 };
        // Die Bricks liegen am oberen Rand des Bandes (Oberkante Brick = Oberkante Band), damit der Roboter
        // sie von oben (um 180° gedreht) greifen kann: Mitte 7 px unter der Oberkante (≤ 10 px zum Greifpunkt).
        public const double RecyclePlatzMitteY = RecycleLineY + BrickHeight / 2;   // 291

        // Größe eines Förderbands in Pixeln (für das Foerderband.png in der View).
        // Die Bricks belegen inkl. Bewegungsweg (10 Schritte * 3 px = 30 px) ca. 104 px;
        // das Band ist länger, damit sie nicht in den runden Enden liegen.
        // Quer zum Band (28 px) liegen die Bricks am inneren Rand (Beweger.bandVersatz), damit der Greifpunkt
        // nah genug an die Brick-Mitte kommt: halbe Brickhöhe (7) + bis zu 2 px Lücke ≤ 10 px.
        public const double ProductionLineWidth = 160;
        public const double ProductionLineHeight = 28;

        // Startpunkt + Winkel jedes Förderbands.
        // Winkel 0  = Band verläuft waagerecht, passend zu left/right.
        // Winkel 90 = Band verläuft senkrecht, passend zu up/down.
        // PLATZHALTER-Koordinaten: an factory_plan.png anpassen!
        public static Pose RectYellowPose = new Pose(125, 450, 0);
        public static Pose RectBluePose   = new Pose(475, 200, 90);
        public static Pose RectRedPose    = new Pose(125, 180, 90);   // Band ab y = 120, unter dem Pfeil "For Recycling"
        public static Pose RectGreenPose  = new Pose(350, 450, 0);

        // Größe eines Legosteins in Pixeln (wie die Brick-Rectangles in MainWindow.axaml: Width="18" Height="14")
        // Gerade Werte, damit die Mitte (9, 7) genau auf einem Pixel liegt; 18 < 20 px Abstand auf der Linie.
        public const int BrickWidth = 18;
        public const int BrickHeight = 14;
    }
