using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
namespace RogueLinux;

public class Game
{
    private string Name { get; }
    private string[] Versions { get; }
    private string ExecutableName { get; }

    public Game(string name, string[] versions, string executableName)
    {
        Name = name;
        Versions = versions;
        ExecutableName = executableName;
    }

    public static Game Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<Game>(json);
    }

    public void Launch()
    {
        Process.Start(Directory.GetCurrentDirectory() + "/" + ExecutableName);
    }

    public static Game[] LoadAllGames()
    {
        throw new NotImplementedException();
    }
}