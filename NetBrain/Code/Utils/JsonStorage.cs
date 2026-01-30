using System.Text.Json;
using NetBrain.Utils;

namespace NetBrain.Code.Utils;

public class JsonStorage<T> where T : new()
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _filePath;

    public JsonStorage(string fileName)
    {
        var folder = StorageUtility.GetPersistentDataPath();
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, fileName);
    }

    public T Load()
    {
        if (!File.Exists(_filePath))
            return new T();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<T>(json, Options) ?? new T();
    }

    public void Save(T data)
    {
        var json = JsonSerializer.Serialize(data, Options);
        File.WriteAllText(_filePath, json);
    }
}
