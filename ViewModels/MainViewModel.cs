using System;
using System.Collections.Generic;
using Avalonia.Threading;
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
        StartTimer();
    }
    private void Timer_Tick(object? sender, EventArgs e)
    {
        factory.LegoBricksBewegen();
        NotifyLegoBricksPositionChanged();
    }

    // Start/Stop der Förderbänder: der Timer bewegt nur die Bricks, der Roboter bleibt immer steuerbar.
    public void StartTimer()
    {
        timer.Start();
    }
    public void StopTimer()
    {
        timer.Stop();
    }

    // Roboter: jede Richtung geht über Factory.RobotBewegen, danach wird die View benachrichtigt.
    public void MoveUp()
    {
        factory.RobotBewegen(Direction.up);
        NotifyRobotPositionChanged();
    }
    public void MoveDown()
    {
        factory.RobotBewegen(Direction.down);
        NotifyRobotPositionChanged();
    }
    public void MoveLeft()
    {
        factory.RobotBewegen(Direction.left);
        NotifyRobotPositionChanged();
    }
    public void MoveRight()
    {
        factory.RobotBewegen(Direction.right);
        NotifyRobotPositionChanged();
    }
    public void MoveUpLeft()
    {
        factory.RobotBewegen(Direction.LeftUp);
        NotifyRobotPositionChanged();
    }
    public void MoveUpRight()
    {
        factory.RobotBewegen(Direction.RightUp);
        NotifyRobotPositionChanged();
    }
    public void MoveDownLeft()
    {
        factory.RobotBewegen(Direction.LeftDown);
        NotifyRobotPositionChanged();
    }
    public void MoveDownRight()
    {
        factory.RobotBewegen(Direction.RightDown);
        NotifyRobotPositionChanged();
    }
    public void MoveRotateLeft()
    {
        factory.RobotBewegen(Direction.rotateLeft);
        NotifyRobotPositionChanged();
    }
    public void MoveRotateRight()
    {
        factory.RobotBewegen(Direction.rotateRight);
        NotifyRobotPositionChanged();
    }
    public void ResetRotation()
    {
        factory.RobotAngle = 0;
        NotifyRobotPositionChanged();
    }
    // Roboter bleibt stehen (Direction.stop) – die Förderbänder laufen davon unabhängig weiter.
    public void Stop()
    {
        factory.RobotBewegen(Direction.stop);
        NotifyRobotPositionChanged();
    }
    private void NotifyRobotPositionChanged()
    {
        OnPropertyChanged(nameof(RobotX));
        OnPropertyChanged(nameof(RobotY));
        OnPropertyChanged(nameof(RobotAngle));
    }
    private void NotifyLegoBricksPositionChanged()
    {
        foreach (var brick in YellowBricks) brick.Refresh();
        foreach (var brick in RedBricks) brick.Refresh();
        foreach (var brick in BlueBricks) brick.Refresh();
        foreach (var brick in GreenBricks) brick.Refresh();
    }
}
