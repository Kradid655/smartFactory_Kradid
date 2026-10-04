using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using smartFactory.Models;
using static smartFactory.Models.Parameters;

namespace MyAvaloniaApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly Robot robot;

    public double RobotX     => robot.Location.X;
    public double RobotY     => robot.Location.Y;
    public double RobotAngle => robot.Angle;

    public MainViewModel()
    {
        robot = new Robot(new Point(550, 500), new Rectangle(545, 235, 520, 515)); //(Startpunkt x;Startpunkt y;Breite;Hoehe)
    }

    private void Move(Direction d)
    {
        robot.Move(d);
        OnPropertyChanged(nameof(RobotX));
        OnPropertyChanged(nameof(RobotY));
    }

    private void Rotate(bool right)
    {
        robot.Rotate(right);
        OnPropertyChanged(nameof(RobotAngle));
    }

    [RelayCommand] private void MoveUp()        => Move(Direction.Up);
    [RelayCommand] private void MoveDown()      => Move(Direction.Down);
    [RelayCommand] private void MoveLeft()      => Move(Direction.Left);
    [RelayCommand] private void MoveRight()     => Move(Direction.Right);
    [RelayCommand] private void MoveUpLeft()    => Move(Direction.UpLeft);
    [RelayCommand] private void MoveUpRight()   => Move(Direction.UpRight);
    [RelayCommand] private void MoveDownLeft()  => Move(Direction.DownLeft);
    [RelayCommand] private void MoveDownRight() => Move(Direction.DownRight);

    [RelayCommand] private void RotateLeft()  => Rotate(false);
    [RelayCommand] private void RotateRight() => Rotate(true);
}
