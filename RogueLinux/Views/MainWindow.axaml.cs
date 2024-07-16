using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace RogueLinux.Views;

public partial class MainWindow : Window
{
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
        var uninstallButton =
            Grid.Children.FirstOrDefault(x => x is Button { HorizontalAlignment: HorizontalAlignment.Left }) as Button;

        button.Content = "Downloading...";
        button.Click -= Install_OnClick;
        button.Click -= Play_OnClick;

        await _selectedGame.DownloadGame();

        button.Content = "Play";
        uninstallButton.Content = "Uninstall";
        uninstallButton.Click += Uninstall_OnClick;
        button.Click += Play_OnClick;
    }

    private void SidebarGame_OnClick(object? sender, RoutedEventArgs e)
    {
        var nameBlock = Grid.Children.FirstOrDefault(x => x is TextBlock) as TextBlock;
        var button = Grid.Children.FirstOrDefault(x => x is Button) as Button;
        var uninstallButton =
            Grid.Children.FirstOrDefault(x => x is Button { HorizontalAlignment: HorizontalAlignment.Left }) as Button;
        var background = Grid.Children.FirstOrDefault(x => x is Image) as Image;
        var descBlock = Grid.Children.FirstOrDefault(x => x is TextBlock { FontSize: 18 }) as TextBlock;

        _selectedGame = (sender as Button).DataContext as Game; // typecasting jumpscare

        // need to remove all event handlers to be sure none fire twice
        button.Click -= NoGame_OnClick;
        button.Click -= Install_OnClick;
        button.Click -= Play_OnClick;
        uninstallButton.Click -= NoGame_OnClick;
        uninstallButton.Click -= Uninstall_OnClick;

        // First: change text and function of button(s) depending on if game is installed
        if (_selectedGame.IsInstalled())
        {
            button.Content = "Play";
            button.Click -= Install_OnClick;
            button.Click += Play_OnClick;
            uninstallButton.Click += Uninstall_OnClick;
            uninstallButton.Content = "Uninstall";
        }
        else
        {
            button.Content = "Install";
            button.Click -= Play_OnClick;
            button.Click += Install_OnClick;
            uninstallButton.Content = "Game Not Installed";
        }

        // Next: game title and background
        background.Source = _selectedGame.Img;
        nameBlock.Text = _selectedGame.Name;
        descBlock.Text = _selectedGame.Description;
    }

    private async void NoGame_OnClick(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("No Game Selected!",
            "Please select a game from the sidebar.");
        await box.ShowAsync();
    }

    private async void Uninstall_OnClick(object? sender, RoutedEventArgs e)
    {
        var button = Grid.Children.FirstOrDefault(x => x is Button) as Button;
        var uninstallButton =
            Grid.Children.FirstOrDefault(x => x is Button { HorizontalAlignment: HorizontalAlignment.Left }) as Button;

        var box = MessageBoxManager.GetMessageBoxStandard("Uninstaller", "Uninstall game?",
            ButtonEnum.YesNo);
        var result = await box.ShowAsync();

        if (result == ButtonResult.Yes)
        {
            button.Click -= Play_OnClick;
            button.Click += Install_OnClick;
            button.Content = "Install";

            uninstallButton.Click -= Uninstall_OnClick;
            uninstallButton.Content = "No Game Installed";
            await _selectedGame.Uninstall();
        }
    }
}