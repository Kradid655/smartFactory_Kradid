using Avalonia.Controls;
using Avalonia.Interactivity;
using smartFactoryApp.ViewModels;

namespace smartFactoryApp.Views;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
    }
    private MainViewModel? ViewModel => DataContext as MainViewModel;

    // Bewegungsmodus wählen
    private void StepMode_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.StepMode();
    }
    private void MoveMode_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.MoveMode();
    }

    // Roboter bewegen
    private void MoveUp_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.MoveUp();
    }
    private void MoveDown_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.MoveDown();
    }
    private void MoveLeft_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.MoveLeft();
    }
    private void MoveRight_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.MoveRight();
    }
    private void MoveUpLeft_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.MoveUpLeft();
    }
    private void MoveUpRight_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.MoveUpRight();
    }
    private void MoveDownLeft_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.MoveDownLeft();
    }
    private void MoveDownRight_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.MoveDownRight();
    }
    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.Stop();
    }

    // Roboter drehen
    private void MoveRotateLeft_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.MoveRotateLeft();
    }
    private void MoveRotateRight_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.MoveRotateRight();
    }
    private void ResetRotation_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.ResetRotation();
    }

    // Förderbänder starten/stoppen
    private void StartTimer_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.StartTimer();
    }
    private void StopTimer_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.StopTimer();
    }

    // Gedockten Legostein loslassen (angedockt wird automatisch)
    private void UndockButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.Undock();
    }
}
