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
    // Wo sich ein Brick gerade befindet / was mit ihm passiert.
    public enum BrickStatus
    {
        aufLinie = 0,      // auf seiner Produktionslinie (andockbar)
        gedockt = 1,       // hängt am Roboter
        zumKunden = 2,     // fährt auf dem Lieferband nach rechts zum Kunden
        zumRecycling = 3,  // defekt, fährt nach links und zur Recycling-Linie
        imRecycling = 4,   // liegt repariert auf der Recycling-Linie (andockbar)
        ausgeliefert = 5,  // beim Kunden angekommen → Linie produziert Ersatz
    }
    // Ergebnis des Abdockens, wird im Fenster als Rückmeldung angezeigt.
    public enum UndockResult
    {
        zumKunden = 0,
        zumRecycling = 1,
        keinBrick = 2,
        nichtAufDeliveryLinie = 3,
        recyclingVoll = 4,
    }
    public const int bricksPerLine = 4;
    public const double rotationStep = 10;
    public const int timerInterval = 50; //Timer-Intervall in Millisekunden
    public const int step = 3;
}