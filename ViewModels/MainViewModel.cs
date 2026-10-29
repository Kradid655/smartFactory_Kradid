using System;
using System.Collections.Generic;
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

    // Pro Farbe eine Liste von LegoBrickViewModels (ersetzt das alte BricksView).
    public List<LegoBrickViewModel> YellowBricks { get; } = new List<LegoBrickViewModel>();
    public List<LegoBrickViewModel> RedBricks { get; } = new List<LegoBrickViewModel>();
    public List<LegoBrickViewModel> BlueBricks { get; } = new List<LegoBrickViewModel>();
    public List<LegoBrickViewModel> GreenBricks { get; } = new List<LegoBrickViewModel>();

    // Ausgelieferte Bricks je Farbe (Anzeige "Delivered")
    public int YellowDelivered => factory.Delivered(LegoColor.yellow);
    public int RedDelivered => factory.Delivered(LegoColor.red);
    public int BlueDelivered => factory.Delivered(LegoColor.blue);
    public int GreenDelivered => factory.Delivered(LegoColor.green);

    // Rückmeldung des Undock-Buttons
    [ObservableProperty] private string undockInfo = "";

    public MainViewModel()
    {
        factory = new Factory();

        // Jeden LegoBrick aus dem Model in ein LegoBrickViewModel verpacken, das die View binden kann.
        foreach (LegoBrick brick in factory.YellowBricks) YellowBricks.Add(new LegoBrickViewModel(brick));
        foreach (LegoBrick brick in factory.RedBricks) RedBricks.Add(new LegoBrickViewModel(brick));
        foreach (LegoBrick brick in factory.BlueBricks) BlueBricks.Add(new LegoBrickViewModel(brick));
        foreach (LegoBrick brick in factory.GreenBricks) GreenBricks.Add(new LegoBrickViewModel(brick));

        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(timerInterval);
        timer.Tick += Timer_Tick;
        // Der Timer startet erst über den START-Button.
    }
    private void Timer_Tick(object? sender, EventArgs e)
    {
        factory.LegoBricksBewegen();
        NotifyLegoBricksPositionChanged();
        OnPropertyChanged(nameof(YellowDelivered));
        OnPropertyChanged(nameof(RedDelivered));
        OnPropertyChanged(nameof(BlueDelivered));
        OnPropertyChanged(nameof(GreenDelivered));

        // Move-Modus: der Roboter fährt bei jedem Tick einen Schritt in die gewählte Richtung.
        if (!IsStepMode && factory.RobotDirection != Direction.stop)
        {
            factory.RobotBewegen(factory.RobotDirection);
            NotifyRobotPositionChanged();
        }
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
            NotifyRobotPositionChanged();
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
        NotifyRobotPositionChanged();
    }
    // Gedockten Brick auf der Delivery-Linie loslassen; der Roboter fährt ein Stück zurück.
    // Das Ergebnis wird als Text unter dem Undock-Button angezeigt.
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
        NotifyRobotPositionChanged();
    }
    // Ein gedockter Brick bewegt sich mit dem Roboter → auch die Bricks neu anzeigen (auch bei gestopptem Timer).
    private void NotifyRobotPositionChanged()
    {
        OnPropertyChanged(nameof(RobotX));
        OnPropertyChanged(nameof(RobotY));
        OnPropertyChanged(nameof(RobotAngle));
        NotifyLegoBricksPositionChanged();
    }
    private void NotifyLegoBricksPositionChanged()
    {
        foreach (var brick in YellowBricks) brick.Refresh();
        foreach (var brick in RedBricks) brick.Refresh();
        foreach (var brick in BlueBricks) brick.Refresh();
        foreach (var brick in GreenBricks) brick.Refresh();
    }
}
