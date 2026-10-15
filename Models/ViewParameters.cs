using System.Drawing;

namespace smartFactoryApp.Models;


    public static class ViewParameters
    {
        public static Pose RobotPose = new Pose(300, 300, 0);
        public static Rectangle Borders = new Rectangle(37, 37, 452, 480);       // Grenzen für den Roboter (70x50)
        public static Rectangle BrickBorders = new Rectangle(37, 37, 510, 510);  // Grenzen für die Bricks: graue Fabrikfläche

        // Größe eines Förderbands in Pixeln (für das Foerderband.png in der View).
        // Länge = 4 Bricks (3 * 20 px Abstand + 16 px Brick) + Bewegungsweg (10 Schritte * 3 px diagonal ≈ 42 px)
        // + 6 px Überstand an jedem Ende.
        public const double ProductionLineWidth = 130;
        public const double ProductionLineHeight = 28;

        // Startpunkt + Winkel jedes Förderbands.
        // Winkel 45  = Band verläuft "\" (links oben → rechts unten), passend zu LeftUp/RightDown.
        // Winkel -45 = Band verläuft "/" (links unten → rechts oben), passend zu RightUp/LeftDown.
        // PLATZHALTER-Koordinaten: an factory_plan.png anpassen!
        public static Pose RectYellowPose = new Pose(125, 450, 45);
        public static Pose RectBluePose   = new Pose(425, 100, 45);
        public static Pose RectRedPose    = new Pose(125, 150, -45);
        public static Pose RectGreenPose  = new Pose(400, 500, -45);
    }
