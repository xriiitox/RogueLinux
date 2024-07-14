using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
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

    static Game Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<Game>(json);
    }

    public async Task DownloadGame()
    {
        if (!IsInstalled())
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Installing Game...", $"Installing {Name} {Version}");
            var installBoxTask = box.ShowAsync();
            Directory.CreateDirectory($"./Games/{Name}/{Version}");
            using var client = new HttpClient();
            await using var s = await client.GetStreamAsync(_downloadPath);
            await using var fs = new FileStream($"./Games/{Name}/{Version}/{_fileName}", FileMode.OpenOrCreate);
            await s.CopyToAsync(fs);

            if (_fileName.Contains("tar.gz") || _fileName.Contains(".tgz"))
            {
                await using var compressedSource = new FileStream($"./Games/{Name}/{Version}/{_fileName}" , FileMode.Open, FileAccess.Read);
                await using MemoryStream memoryStream = new MemoryStream();
                await using (GZipStream gzipStream = 
                             new(compressedSource, CompressionMode.Decompress))
                {
                    await gzipStream.CopyToAsync(memoryStream);
                }
                memoryStream.Seek(0, SeekOrigin.Begin);
                await TarFile.ExtractToDirectoryAsync(
                        memoryStream,
                        $"./Games/{Name}/{Version}/",
                        overwriteFiles: true
                    );
                File.Delete($"./Games/{Name}/{Version}/{_fileName}");
            } 
            else if (_fileName.Contains(".zip"))
            {
                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory($"./Games/{Name}/{Version}/{_fileName}", $"./Games/{Name}/{Version}/", true);
                });
            }
            
            await installBoxTask;
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

    public bool IsInstalled()
    {
        return Directory.Exists($"./Games/{Name}/{Version}");
    }
}