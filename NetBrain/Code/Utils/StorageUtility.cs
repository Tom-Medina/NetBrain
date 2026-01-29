namespace NetBrain.Utils;

public static class StorageUtility
{
    public static string GetPersistentDataPath()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(basePath, "NetBrain");
    }
}
