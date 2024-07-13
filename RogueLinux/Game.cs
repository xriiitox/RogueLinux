using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using MsBox.Avalonia;
using Newtonsoft.Json;
namespace RogueLinux;

public class Game
{
     public string Name { get; }
     public string Version { get; }
     public string ExecutableName { get; }
     public string DownloadPath { get; }
     public string FileName { get; }
     public string ImgName { get; }


    public Game(string name, string version, string executableName, string downloadPath, string fileName, string imgName)
    {
        Name = name;
        Version = version;
        ExecutableName = executableName;
        DownloadPath = downloadPath;
        FileName = fileName;
        ImgName = imgName;
    }

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
            await using var s = await client.GetStreamAsync(DownloadPath);
            await using var fs = new FileStream(FileName, FileMode.OpenOrCreate);
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
        Process.Start(Directory.GetCurrentDirectory() + $"/Games/{Name}/{Version}/" + ExecutableName);
    }

    public static List<Game> LoadAllGames()
    {
        string[] files = 
            Directory.GetFiles("./json/", "*.json", SearchOption.AllDirectories);
        
        List<Game> games = new List<Game>();

        foreach (var file in files)
        {
            games.Append(Deserialize(File.ReadAllText(file)));
        }

        return games;

    }
}