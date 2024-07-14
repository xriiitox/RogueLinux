using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Animation;
using RogueLinux.ViewModels;

namespace RogueLinux.Views;

public partial class MainWindow : Window
{
    MainWindowViewModel vm = new();
    private Game _selectedGame;
    
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Play_OnClick(object? sender, RoutedEventArgs e)
    {
        _selectedGame.Launch();
    }

    private async void Install_OnClick(object? sender, RoutedEventArgs e)
    {
        var button = Grid.Children.FirstOrDefault(x => x is Button) as Button;
        
        button.Content = "Downloading...";
        button.Click -= Install_OnClick;
        button.Click -= Play_OnClick;
        
        await _selectedGame.DownloadGame();
        
        button.Content = "Play";
        button.Click += Play_OnClick;
    }

    private void SidebarGame_OnClick(object? sender, RoutedEventArgs e)
    {
        var nameBlock = Grid.Children.FirstOrDefault(x => x is TextBlock) as TextBlock;
        var button = Grid.Children.FirstOrDefault(x => x is Button) as Button;
        var background = Grid.Background as ImageBrush;
        
        _selectedGame = (sender as Button).DataContext as Game; // i dont know man
        
        // First: change text and function of button depending on if game is installed
        if (_selectedGame.IsInstalled())
        {
            button.Content = "Play";
            button.Click -= Install_OnClick;
            button.Click += Play_OnClick;
        }
        else
        {
            button.Content = "Install";
            button.Click -= Play_OnClick;
            button.Click += Install_OnClick;
        }
        
        // Next: game title and background
        background.Source = _selectedGame.Img;
        nameBlock.Text = _selectedGame.Name;
        
    }

    private async void SidebarGame_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        var animation = Resources["ListItemAnimation"] as Animation;
        await animation.RunAsync(sender as Button);
    }
}