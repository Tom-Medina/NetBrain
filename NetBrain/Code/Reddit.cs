using System.Text.Json;
using NetBrain.Utils;

namespace NetBrain.Code;

public class Reddit
{
    private static readonly string[] Subreddits =
    [
        "roguelikes",
        "indiegaming",
        "roguelites",
        "ARPG",
        "GamesLikeDiablo",
        "gamingvids",
        "GamerVideos",
        "gamerecommendations",
        "GamesWePlay",
        "videogamescience",
        "RPGreview",
        "patientgamers"
    ];

    private readonly string _indexFile;
    private int _currentIndex;

    public Reddit()
    {
        var folder = StorageUtility.GetPersistentDataPath();
        Directory.CreateDirectory(folder);
        _indexFile = Path.Combine(folder, "reddit_index.json");
        _currentIndex = LoadIndex();
    }

    public string GetNextSubreddit()
    {
        var subreddit = Subreddits[_currentIndex % Subreddits.Length];
        _currentIndex = (_currentIndex + 1) % Subreddits.Length;
        SaveIndex();
        return subreddit;
    }

    private int LoadIndex()
    {
        if (!File.Exists(_indexFile))
            return 0;

        var json = File.ReadAllText(_indexFile);
        return JsonSerializer.Deserialize<int>(json);
    }

    private void SaveIndex()
    {
        var json = JsonSerializer.Serialize(_currentIndex);
        File.WriteAllText(_indexFile, json);
    }
}
