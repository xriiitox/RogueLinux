using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Avalonia.Media.Imaging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using MsBox.Avalonia;
using Newtonsoft.Json;

namespace RogueLinux;

public class Game : ObservableObject
{
    private readonly string _downloadPath;
    private readonly string _executableName;
    private readonly string _fileName;


    public Game(string name, string version, string executableName, string downloadPath, string fileName,
        string imgName)
    {
        Name = name;
        Version = version;
        _executableName = executableName;
        _downloadPath = downloadPath;
        _fileName = fileName;
        Img = ImageHelper.LoadFromResource(new Uri($"avares://{typeof(Program).Assembly.GetName().Name}/Assets/{imgName}"));
    }

    public string Name { get; }
    public string Version { get; }

    public Bitmap? Img { get; }

    public static Game Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<Game>(json);
    }

    public async void DownloadGame()
    {
        if (!Directory.Exists($"./Games/{Name}/{Version}"))
        {
            Directory.CreateDirectory($"./Games/{Name}");
            Directory.CreateDirectory($"./Games/{Name}/{Version}");
            using var client = new HttpClient();
            await using var s = await client.GetStreamAsync(_downloadPath);
            await using var fs = new FileStream(_fileName, FileMode.OpenOrCreate);
            await s.CopyToAsync(fs);
        }
        else
        {
            // probably redundant as this function will never be run if the game is installed already
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Game already exists");
            await box.ShowAsync();
        }
    }

    public void Launch()
    {
        Process.Start(Directory.GetCurrentDirectory() + $"/Games/{Name}/{Version}/" + _executableName);
    }

    public static List<Game> LoadAllGames()
    {
        var files =
            Directory.GetFiles("./json/", "*.json", SearchOption.AllDirectories);

        var games = new List<Game>();

        foreach (var file in files) games.Add(Deserialize(File.ReadAllText(file)));

        return games;
    }
}