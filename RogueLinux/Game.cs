using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using MsBox.Avalonia;
using Newtonsoft.Json;

namespace RogueLinux;

[method: JsonConstructor]
public class Game(
    string name,
    string version,
    string executableName,
    string downloadPath,
    string workingDir,
    string fileName,
    string imgName,
    string description)
    : ObservableObject
{
    public string Name { get; } = name;
    public string Version { get; } = version;
    public Bitmap? Img { get; } = ImageHelper.LoadFromResource(
        new Uri($"avares://{typeof(Program).Assembly.GetName().Name}/Assets/{imgName}"));

    public string Description { get; } = description;

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
            await using var s = await client.GetStreamAsync(downloadPath);
            await using var fs = new FileStream($"./Games/{Name}/{Version}/{fileName}", FileMode.OpenOrCreate);
            await s.CopyToAsync(fs);

            if (fileName.Contains("tar.gz") || fileName.Contains(".tgz"))
            {
                await using var compressedSource = new FileStream($"./Games/{Name}/{Version}/{fileName}",
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
                File.Delete($"./Games/{Name}/{Version}/{fileName}");
            }
            else if (fileName.Contains(".zip"))
            {
                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory($"./Games/{Name}/{Version}/{fileName}",
                        $"./Games/{Name}/{Version}/", true);
                });
            }
            else
            {
#pragma warning disable CA1416 // Will not work on windows
                File.SetUnixFileMode($"./Games/{Name}/{Version}/{fileName}",
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
        if (fileName.Contains(".jar"))
        {
            var currentDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory($"./Games/{Name}/{Version}/{workingDir}");
            Process.Start("/usr/bin/env", "java -jar " + executableName);
            Directory.SetCurrentDirectory(currentDir);
        }
        else
        {
            var currentDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory($"./Games/{Name}/{Version}/{workingDir}");
            Process.Start(executableName);
            Directory.SetCurrentDirectory(currentDir);
        }
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

    public async Task Uninstall()
    {
        await Task.Run(() => { Directory.Delete($"./Games/{Name}/{Version}", true); });
    }
}