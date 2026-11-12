using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using smartFactoryApp.Models;
using static smartFactoryApp.Models.Parameters;

namespace smartFactoryApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    Factory factory;
    DispatcherTimer timer;
    public double RobotX => factory.RobotX;
    public double RobotY => factory.RobotY;
    public double RobotAngle => factory.RobotAngle;

    // Förderbänder bewegen sich nicht → reines Durchreichen, kein OnPropertyChanged nötig.
    public double ProductionYellowX => factory.ProductionYellowX;
    public double ProductionYellowY => factory.ProductionYellowY;
    public double ProductionYellowAngle => factory.ProductionYellowAngle;
    public double ProductionRedX => factory.ProductionRedX;
    public double ProductionRedY => factory.ProductionRedY;
    public double ProductionRedAngle => factory.ProductionRedAngle;
    public double ProductionBlueX => factory.ProductionBlueX;
    public double ProductionBlueY => factory.ProductionBlueY;
    public double ProductionBlueAngle => factory.ProductionBlueAngle;
    public double ProductionGreenX => factory.ProductionGreenX;
    public double ProductionGreenY => factory.ProductionGreenY;
    public double ProductionGreenAngle => factory.ProductionGreenAngle;

    // Pro Linie eine Liste von LegoBrickViewModels. Am Anfang leer (die Linien haben noch keine Bricks),
    // gefüllt wird sie beim ersten "Setup Production Lines".
    public List<LegoBrickViewModel> YellowBricks { get; private set; } = new List<LegoBrickViewModel>();
    public List<LegoBrickViewModel> RedBricks { get; private set; } = new List<LegoBrickViewModel>();
    public List<LegoBrickViewModel> BlueBricks { get; private set; } = new List<LegoBrickViewModel>();
    public List<LegoBrickViewModel> GreenBricks { get; private set; } = new List<LegoBrickViewModel>();

    // Bricks erst nach "Setup Production Lines" anzeigen
    [ObservableProperty] private bool bricksVisible;

    // Bestellung (Order) und ausgelieferte Bricks je Farbe
    public int YellowOrder => factory.GetOrdered(LegoColor.yellow);
    public int BlueOrder => factory.GetOrdered(LegoColor.blue);
    public int RedOrder => factory.GetOrdered(LegoColor.red);
    public int GreenOrder => factory.GetOrdered(LegoColor.green);
    public int YellowDelivered => factory.GetDelivered(LegoColor.yellow);
    public int RedDelivered => factory.GetDelivered(LegoColor.red);
    public int BlueDelivered => factory.GetDelivered(LegoColor.blue);
    public int GreenDelivered => factory.GetDelivered(LegoColor.green);

    // Status der Bestellung
    public string OrderInfo
    {
        get
        {
            if (!factory.HasOrder)
                return "No order yet: click \"Order\"";
            if (factory.OrderComplete)
                return "Order complete!";
            string info = $"Order in progress: {factory.DeliveredTotal} of {factory.OrderedTotal} delivered";
            if (factory.LastDelivery == DeliveryResult.nichtBestellt)
                info += "\n(last brick was not part of the order)";
            return info;
        }
    }

    // Logistik-Plan: welcher Brick als Nächstes von welcher Linie geholt wird
    public string LogisticsPlan
    {
        get
        {
            LogisticsStep schritt = factory.LogisticsStep;
            string farbe = schritt.Brick?.Color.ToString() ?? "";
            return schritt.Art switch
            {
                LogisticsStepKind.keineOrder => "Waiting for an order.",
                LogisticsStepKind.linienLeer => "Production lines are empty: click \"Setup Production Lines\".",
                LogisticsStepKind.abliefern => $"Bring the {farbe} brick to the Delivery Line (GoToDelivery, then Undock).",
                LogisticsStepKind.recyclingAbholen => $"PRIORITY: pick up the {farbe} brick from the Recycle Line.",
                LogisticsStepKind.abholen => $"Pick up a {farbe} brick from the {LineName(schritt.Line!.Color)}.",
                LogisticsStepKind.warten => "Wait: the remaining bricks are on their way to the customer.",
                LogisticsStepKind.keinPassenderBrick => "No needed brick on the lines: click \"Setup Production Lines\" for new bricks.",
                LogisticsStepKind.orderKomplett => "Order complete: click \"Order\" for a new order.",
                _ => "",
            };
        }
    }

    // Die Linien heißen nach ihrer Kennfarbe; die Bricks darauf haben zufällige Farben.
    private static string LineName(LegoColor line) => line switch
    {
        LegoColor.yellow => "Yellow Line (bottom left)",
        LegoColor.green => "Green Line (bottom right)",
        LegoColor.red => "Red Line (left)",
        _ => "Blue Line (right)",
    };

    // Rückmeldung zu Undock, GoToPickUp und GoToDelivery
    [ObservableProperty] private string undockInfo = "";

    public MainViewModel()
    {
        factory = new Factory();

        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(timerInterval);
        timer.Tick += Timer_Tick;
        // Der Timer startet erst über den START-Button.
    }
    private void Timer_Tick(object? sender, EventArgs e)
    {
        factory.LegoBricksBewegen();

        // Move-Modus: der Roboter fährt bei jedem Tick einen Schritt in die gewählte Richtung.
        if (!IsStepMode && factory.RobotDirection != Direction.stop)
            factory.RobotBewegen(factory.RobotDirection);

        NotifyAll();
    }

    // START/STOP: der Timer bewegt die Bricks und im Move-Modus auch den Roboter.
    // Bei STOP bleibt der Roboter stehen und fährt nach START nicht von selbst weiter.
    public void StartTimer()
    {
        timer.Start();
    }
    public void StopTimer()
    {
        timer.Stop();
        factory.RobotDirection = Direction.stop;
    }

    // Linien mit Bricks in Zufallsfarben bestücken. Beim ersten Mal werden die LegoBrickViewModels angelegt.
    public void SetupProductionLines()
    {
        factory.SetupProductionLines();
        if (!BricksVisible)
        {
            YellowBricks = factory.YellowBricks.Select(b => new LegoBrickViewModel(b)).ToList();
            RedBricks = factory.RedBricks.Select(b => new LegoBrickViewModel(b)).ToList();
            BlueBricks = factory.BlueBricks.Select(b => new LegoBrickViewModel(b)).ToList();
            GreenBricks = factory.GreenBricks.Select(b => new LegoBrickViewModel(b)).ToList();
            OnPropertyChanged(nameof(YellowBricks));
            OnPropertyChanged(nameof(RedBricks));
            OnPropertyChanged(nameof(BlueBricks));
            OnPropertyChanged(nameof(GreenBricks));
            BricksVisible = true;
        }
        NotifyAll();
    }

    // Neue zufällige Bestellung (maximal 4 je Farbe)
    public void Order()
    {
        factory.CreateOrder();
        NotifyAll();
    }

    // Teleport zu der Linie, die der Logistik-Plan anzeigt (Roboter schaut auf die Linie)
    public void GoToPickUp()
    {
        LogisticsStepKind art = factory.LogisticsStep.Art;
        if (factory.GoToLine())
            UndockInfo = art == LogisticsStepKind.recyclingAbholen
                ? "Robot is at the Recycle Line: move forward (Up) to pick up the brick."
                : "Robot is at the production line: move forward (Up) when the right brick is in front.";
        else
            UndockInfo = art switch
            {
                LogisticsStepKind.abholen or LogisticsStepKind.recyclingAbholen => "GoToPickUp: target position is blocked.",
                LogisticsStepKind.abliefern => "GoToPickUp: robot already carries a brick, use GoToDelivery.",
                _ => "GoToPickUp: nothing to pick up right now (see logistics plan).",
            };
        NotifyAll();
    }

    // Teleport zur Delivery-Linie
    public void GoToDelivery()
    {
        UndockInfo = factory.GoToDelivery()
            ? "Robot is at the Delivery Line."
            : "GoToDelivery: target position is blocked.";
        NotifyAll();
    }

    // Step-Modus: ein Klick = ein Schritt (Standard).
    // Move-Modus: ein Klick legt nur die Richtung fest, gefahren wird im Timer_Tick (also nur nach START).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMoveMode))]
    private bool isStepMode = true;
    public bool IsMoveMode => !IsStepMode;

    public void StepMode()
    {
        IsStepMode = true;
        factory.RobotDirection = Direction.stop;
    }
    public void MoveMode()
    {
        IsStepMode = false;
    }

    // Jede Richtung geht über RobotSteuern: im Step-Modus sofort einen Schritt, im Move-Modus Richtung merken.
    private void RobotSteuern(Direction dir)
    {
        if (IsStepMode)
        {
            factory.RobotBewegen(dir);
            NotifyAll();
        }
        else
        {
            factory.RobotDirection = dir;
        }
    }

    public void MoveUp() => RobotSteuern(Direction.up);
    public void MoveDown() => RobotSteuern(Direction.down);
    public void MoveLeft() => RobotSteuern(Direction.left);
    public void MoveRight() => RobotSteuern(Direction.right);
    public void MoveUpLeft() => RobotSteuern(Direction.LeftUp);
    public void MoveUpRight() => RobotSteuern(Direction.RightUp);
    public void MoveDownLeft() => RobotSteuern(Direction.LeftDown);
    public void MoveDownRight() => RobotSteuern(Direction.RightDown);
    public void MoveRotateLeft() => RobotSteuern(Direction.rotateLeft);
    public void MoveRotateRight() => RobotSteuern(Direction.rotateRight);
    // Roboter bleibt stehen (Direction.stop) – die Förderbänder laufen davon unabhängig weiter.
    public void Stop() => RobotSteuern(Direction.stop);

    public void ResetRotation()
    {
        factory.RobotAngle = 0;
        NotifyAll();
    }
    // Gedockten Brick auf der Delivery-Linie loslassen; der Roboter fährt ein Stück zurück.
    public void Undock()
    {
        UndockInfo = factory.Undock() switch
        {
            UndockResult.zumKunden => "Undocking successful: brick goes to the customer.",
            UndockResult.zumRecycling => "Undocking successful, but brick is defective: sent to recycling.",
            UndockResult.keinBrick => "No brick available to undock.",
            UndockResult.nichtAufDeliveryLinie => "Undocking is only possible on the Delivery Line.",
            UndockResult.recyclingVoll => "Recycling Line is full: undocking not possible.",
            _ => "",
        };
        NotifyAll();
    }

    // Alles, was sich durch Timer, Roboter oder Buttons ändern kann, neu anzeigen
    // (ein gedockter Brick bewegt sich mit dem Roboter → auch bei gestopptem Timer).
    private void NotifyAll()
    {
        OnPropertyChanged(nameof(RobotX));
        OnPropertyChanged(nameof(RobotY));
        OnPropertyChanged(nameof(RobotAngle));
        foreach (var brick in YellowBricks.Concat(RedBricks).Concat(BlueBricks).Concat(GreenBricks))
            brick.Refresh();

        OnPropertyChanged(nameof(YellowOrder));
        OnPropertyChanged(nameof(BlueOrder));
        OnPropertyChanged(nameof(RedOrder));
        OnPropertyChanged(nameof(GreenOrder));
        OnPropertyChanged(nameof(YellowDelivered));
        OnPropertyChanged(nameof(BlueDelivered));
        OnPropertyChanged(nameof(RedDelivered));
        OnPropertyChanged(nameof(GreenDelivered));
        OnPropertyChanged(nameof(OrderInfo));
        OnPropertyChanged(nameof(LogisticsPlan));
    }
}
