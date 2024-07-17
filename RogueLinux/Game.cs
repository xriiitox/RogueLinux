using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using MsBox.Avalonia;
using Newtonsoft.Json;

namespace RogueLinux;

public class Game : ObservableObject
{
    private readonly string _executableName;
    private readonly string _downloadPath;
    private readonly string _workingDir;
    private readonly string _fileName;
    public string Name { get; }
    public string Version { get; }
    public Bitmap? Img { get; }
    
    Game() { } // json crap needs this for some reason

    
    public Game(string name,
        string version,
        string executableName,
        string downloadPath,
        string workingDir,
        string fileName,
        string imgName,
        string description)
    {
        _executableName = executableName;
        _downloadPath = downloadPath;
        _workingDir = workingDir;
        _fileName = fileName;
        Name = name;
        Version = version;
        Img = new Bitmap(AssetLoader.Open(new Uri($"avares://{Assembly.GetExecutingAssembly().GetName().Name}/Assets/{imgName}")));
        Description = description;
    }

    public string Description { get; }

    private static Game Deserialize(string json)
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
                await using var compressedSource = new FileStream($"./Games/{Name}/{Version}/{_fileName}",
                    FileMode.Open, FileAccess.Read);
                await using var memoryStream = new MemoryStream();
                await using (GZipStream gzipStream =
                             new(compressedSource, CompressionMode.Decompress))
                {
                    await gzipStream.CopyToAsync(memoryStream);
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                await TarFile.ExtractToDirectoryAsync(
                    memoryStream,
                    $"./Games/{Name}/{Version}/",
                    true
                );
                File.Delete($"./Games/{Name}/{Version}/{_fileName}");
            }
            else if (_fileName.Contains(".zip"))
            {
                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory($"./Games/{Name}/{Version}/{_fileName}",
                        $"./Games/{Name}/{Version}/", true);
                });
            }
            else
            {
#pragma warning disable CA1416 // Will not work on windows
                File.SetUnixFileMode($"./Games/{Name}/{Version}/{_fileName}",
                    UnixFileMode.UserExecute |
                    UnixFileMode.GroupExecute |
                    UnixFileMode.OtherExecute |
                    UnixFileMode.UserRead |
                    UnixFileMode.GroupRead |
                    UnixFileMode.OtherRead |
                    UnixFileMode.UserWrite |
                    UnixFileMode.GroupWrite |
                    UnixFileMode.OtherWrite);
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
        if (_fileName.Contains(".jar"))
        {
            var currentDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory($"./Games/{Name}/{Version}/{_workingDir}");
            Process.Start("/usr/bin/env", "java -jar " + _executableName);
            Directory.SetCurrentDirectory(currentDir);
        }
        else
        {
            var currentDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory($"./Games/{Name}/{Version}/{_workingDir}");
            Process.Start(_executableName);
            Directory.SetCurrentDirectory(currentDir);
        }
    }

    public static List<Game> LoadAllGames()
    {
        List<string> files = [];

        foreach (var file in AssetLoader.GetAssets(new Uri("avares://RogueLinux/Assets/json/"), null))
        {
            using StreamReader reader = new StreamReader(AssetLoader.Open(file));
            string json = reader.ReadToEnd();
            files.Add(json);
        }

        return files.Select(Deserialize).ToList();
    }

    public bool IsInstalled()
    {
        return Directory.Exists($"./Games/{Name}/{Version}");
    }

    public async Task Uninstall()
    {
        await Task.Run(() => { Directory.Delete($"./Games/{Name}/{Version}", true); });
    }
}