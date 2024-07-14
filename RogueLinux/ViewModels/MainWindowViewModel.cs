using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace RogueLinux.ViewModels;

public class MainWindowViewModel : ObservableObject
{
    private ObservableCollection<Game> _games = new(Game.LoadAllGames());

    public MainWindowViewModel()
    {
        Games = new ObservableCollection<Game>(Game.LoadAllGames());
    }

    public ObservableCollection<Game> Games
    {
        get => _games;
        set => SetProperty(ref _games, value);
    }
}