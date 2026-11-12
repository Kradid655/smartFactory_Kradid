using System;
using System.Collections.Generic;
using System.Linq;
using static smartFactoryApp.Models.Parameters;

namespace smartFactoryApp.Models;

// Ein Schritt des Logistik-Plans: was der Roboter als Nächstes tun soll,
// mit welchem Brick und (beim Abholen von einer Produktionslinie) von welcher Linie.
public record LogisticsStep(LogisticsStepKind Art, LegoBrick? Brick = null, ProductionLine? Line = null);

// Verwaltet die Bestellung (Order) des Kunden, zählt die ausgelieferten Bricks
// und berechnet den Logistik-Plan: welcher Brick als Nächstes von welcher Linie geholt wird.
public class Logistics
{
    private static readonly LegoColor[] farben = Enum.GetValues<LegoColor>();
    private readonly Random random = new Random();
    private Dictionary<LegoColor, int>? order;                            // null = noch keine Bestellung
    private readonly Dictionary<LegoColor, int> delivered = new Dictionary<LegoColor, int>();
    private LogisticsStep naechsterSchritt = new LogisticsStep(LogisticsStepKind.keineOrder);

    public bool HasOrder => order != null;
    public LogisticsStep CurrentStep => naechsterSchritt;
    public int GetOrdered(LegoColor color) => order?.GetValueOrDefault(color) ?? 0;
    public int GetDelivered(LegoColor color) => delivered.GetValueOrDefault(color);
    public int OrderedTotal => farben.Sum(GetOrdered);
    public int DeliveredTotal => farben.Sum(GetDelivered);
    public bool IsComplete => order != null && farben.All(c => GetDelivered(c) >= GetOrdered(c));

    // Neue Bestellung: je Farbe zufällig 0..orderSize Bricks, insgesamt aber mindestens einer.
    // Die Zählung der ausgelieferten Bricks beginnt von vorn.
    public void CreateRandomOrder()
    {
        do
        {
            order = farben.ToDictionary(c => c, c => random.Next(orderSize + 1));
        } while (order.Values.Sum() == 0);
        delivered.Clear();
    }

    // Ein Brick ist beim Kunden angekommen. Gezählt wird er nur, wenn seine Farbe noch gebraucht wird.
    public DeliveryResult BrickDelivered(LegoColor color)
    {
        if (order == null)
            return DeliveryResult.keineOrder;
        if (GetDelivered(color) >= GetOrdered(color))
            return DeliveryResult.nichtBestellt;
        delivered[color] = GetDelivered(color) + 1;
        return IsComplete ? DeliveryResult.orderKomplett : DeliveryResult.gezaehlt;
    }

    // Muss noch ein Brick dieser Farbe von einer Produktionslinie geholt werden?
    // Offen = bestellt - ausgeliefert - schon unterwegs (am Roboter, zum Kunden, zum/im Recycling).
    public bool IsNeeded(LegoColor color, IEnumerable<LegoBrick> alleBricks)
    {
        int unterwegs = alleBricks.Count(b => b.Color == color && b.Status is BrickStatus.gedockt
            or BrickStatus.zumKunden or BrickStatus.zumRecycling or BrickStatus.imRecycling);
        return GetOrdered(color) - GetDelivered(color) - unterwegs > 0;
    }

    // Berechnet den nächsten Schritt des Plans aus dem aktuellen Zustand der Fabrik.
    public LogisticsStep PickStation(LegoBrick? gedockt, IReadOnlyList<ProductionLine> lines)
    {
        naechsterSchritt = BerechneSchritt(gedockt, lines);
        return naechsterSchritt;
    }

    private LogisticsStep BerechneSchritt(LegoBrick? gedockt, IReadOnlyList<ProductionLine> lines)
    {
        if (order == null)
            return new LogisticsStep(LogisticsStepKind.keineOrder);
        if (gedockt != null)
            return new LogisticsStep(LogisticsStepKind.abliefern, gedockt);
        if (IsComplete)
            return new LogisticsStep(LogisticsStepKind.orderKomplett);

        List<LegoBrick> alle = lines.SelectMany(l => l.Bricks).ToList();
        if (alle.Count == 0)
            return new LogisticsStep(LogisticsStepKind.linienLeer);

        // Priorität: Bricks, die nicht zum Kunden gegangen sind (Recycling), werden sofort wieder abgeholt.
        // Einer, der schon auf der Recycling-Linie liegt, vor einem, der noch dorthin unterwegs ist.
        LegoBrick? recycling = alle
            .Where(b => b.Status is BrickStatus.imRecycling or BrickStatus.zumRecycling)
            .OrderBy(b => b.Status == BrickStatus.imRecycling ? 0 : 1)
            .FirstOrDefault();
        if (recycling != null)
            return new LogisticsStep(LogisticsStepKind.recyclingAbholen, recycling);

        // Sonst: erste benötigte Farbe, die auf einer Produktionslinie liegt.
        List<LegoColor> benoetigt = farben.Where(c => IsNeeded(c, alle)).ToList();
        if (benoetigt.Count == 0)
            return new LogisticsStep(LogisticsStepKind.warten);
        foreach (LegoColor color in benoetigt)
            foreach (ProductionLine line in lines)
            {
                LegoBrick? brick = line.Bricks.FirstOrDefault(b => b.Status == BrickStatus.aufLinie && b.Color == color);
                if (brick != null)
                    return new LogisticsStep(LogisticsStepKind.abholen, brick, line);
            }
        return new LogisticsStep(LogisticsStepKind.keinPassenderBrick);
    }
}
