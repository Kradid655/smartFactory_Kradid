using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using smartFactoryApp.Models;
using static smartFactoryApp.Models.Parameters;

namespace smartFactoryApp.ViewModels;
public partial class LegoBrickViewModel : ViewModelBase
{
    private LegoBrick brick;
    [ObservableProperty] private double x;
    [ObservableProperty] private double y;
    [ObservableProperty] private double angle;
    // Farbe des Bricks: zufällig und kann sich beim Nachproduzieren ändern → wird mit aktualisiert.
    [ObservableProperty] private IBrush fill = Brushes.Gray;

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
        Fill = ToBrush(brick.Color);
    }

    public static IBrush ToBrush(LegoColor color) => color switch
    {
        LegoColor.yellow => Brushes.Gold,
        LegoColor.blue => Brushes.RoyalBlue,
        LegoColor.red => Brushes.Firebrick,
        _ => Brushes.ForestGreen,
    };
}
