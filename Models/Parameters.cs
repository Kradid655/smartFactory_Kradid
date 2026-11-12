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
    // Ergebnis, wenn ein Brick beim Kunden ankommt (Logistics.BrickDelivered).
    public enum DeliveryResult
    {
        keineOrder = 0,     // es gibt noch keine Bestellung → nicht gezählt
        gezaehlt = 1,       // gehört zur Bestellung und wurde gezählt
        nichtBestellt = 2,  // Farbe wird (nicht mehr) gebraucht → nicht gezählt
        orderKomplett = 3,  // gezählt, und damit ist die Bestellung vollständig
    }
    // Was der Logistik-Plan dem Roboter als Nächstes vorgibt.
    public enum LogisticsStepKind
    {
        keineOrder = 0,          // noch keine Bestellung
        linienLeer = 1,          // Produktionslinien noch nicht bestückt
        abliefern = 2,           // Roboter trägt einen Brick → zur Delivery-Linie
        recyclingAbholen = 3,    // Priorität: Brick von der Recycling-Linie holen
        abholen = 4,             // Brick einer benötigten Farbe von einer Produktionslinie holen
        warten = 5,              // alles Benötigte ist schon unterwegs zum Kunden
        keinPassenderBrick = 6,  // benötigte Farbe liegt auf keiner Linie
        orderKomplett = 7,       // Bestellung vollständig ausgeliefert
    }
    public const int bricksPerLine = 4;
    public const int orderSize = 4;   // maximal so viele Bricks je Farbe in einer Bestellung
    public const double rotationStep = 10;
    public const int timerInterval = 50; //Timer-Intervall in Millisekunden
    public const int step = 3;
}