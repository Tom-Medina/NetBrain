using System.Diagnostics;

namespace NetBrain.Code.Commands;

public class UpdateCommand : ITelegramCommand
{
    public string Name => "/update";

    public Task<string> ExecuteAsync(string[] args)
    {
        var scriptPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "NetBrain", "update.sh");

        var process = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = scriptPath,
            UseShellExecute = false,
            RedirectStandardOutput = false,
            CreateNoWindow = true
        };

        Process.Start(process);

        return Task.FromResult("Updating... The bot will restart shortly.");
    }
}
