using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RogueLinux.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var games = Game.LoadAllGames()
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Version);

        var gamesForDisplay = games.Select(x => x.Name);

        foreach (var game in games)
        {
            Games.Items.Add(game);
        }
    }

    private void PlayOrInstall(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }
}