using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using RogueLinux.ViewModels;

namespace RogueLinux.Views;

public partial class MainWindow : Window
{
    MainWindowViewModel vm = new();
    private Game selectedGame;
    
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Play_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Install_OnClick(object? sender, RoutedEventArgs e)
    {
        selectedGame.DownloadGame();
    }

    private void SidebarGame_OnClick(object? sender, RoutedEventArgs e)
    {
        var nameBlock = Grid.Children.FirstOrDefault(x => x is TextBlock) as TextBlock;
        var button = Grid.Children.FirstOrDefault(x => x is Button) as Button;
        var background = Grid.Background as ImageBrush;
        
        selectedGame = (sender as Button).DataContext as Game; // i dont know man
        
        // First: change text and function of button depending on if game is installed
        if (Directory.Exists($"./Games/{selectedGame.Name}/{selectedGame.Version}"))
        {
            button.Content = "Play";
            button.Click += Play_OnClick;
        }
        else
        {
            button.Content = "Install";
            button.Click += Install_OnClick;
        }
        
        // Next: game title and background
        background.Source = selectedGame.Img;
        nameBlock.Text = selectedGame.Name;
        
    }
}