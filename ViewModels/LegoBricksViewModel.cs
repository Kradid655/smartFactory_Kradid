using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using smartFactoryApp.Models;

namespace smartFactoryApp.ViewModels;
public partial class LegoBrickViewModel : ViewModelBase
{
    private LegoBrick brick;
    [ObservableProperty] private double x;
    [ObservableProperty] private double y;
    [ObservableProperty] private double angle;

    public LegoBrickViewModel(LegoBrick brick)
    {
        this.brick = brick;
        Refresh();
    }
    public void Refresh()
    {
        X = brick.Location.X;
        Y = brick.Location.Y;
        Angle = brick.Angle;
    }
}
