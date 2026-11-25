using System.Text.Json;
using Velopack;
using Velopack.Sources;

// Scratch head of agile-canary#686: the updater of profiles/desktop.md, GithubSource with Updates:Prerelease.
VelopackApp.Build().Run();

var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json"))).RootElement.GetProperty("Updates");
var source = settings.GetProperty("Source").GetString() ?? "";
var prerelease = settings.TryGetProperty("Prerelease", out var p) && p.ValueKind == JsonValueKind.True;
var manager = new UpdateManager(new GithubSource(source, null, prerelease));

Console.WriteLine($"App {(manager.IsInstalled ? manager.CurrentVersion?.ToString() : "not installed (run from a folder)")} - reads pre-releases: {(prerelease ? "yes" : "no")}");
if (manager.IsInstalled)
{
    try
    {
        var update = await manager.CheckForUpdatesAsync();
        if (update is null) Console.WriteLine("No update available.");
        else
        {
            Console.WriteLine($"Version {update.TargetFullRelease.Version} is available. Update now? (y/n)");
            if ((Console.ReadLine() ?? "").Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                await manager.DownloadUpdatesAsync(update);
                manager.ApplyUpdatesAndRestart(update);
            }
        }
    }
    catch (Exception e) { Console.WriteLine($"Update check failed, the app keeps its version: {e.Message}"); }
}
Console.WriteLine("Press Enter to close.");
Console.ReadLine();
