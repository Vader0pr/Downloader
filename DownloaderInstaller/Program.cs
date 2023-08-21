using GitHubAPIWrapper.Releases;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Timers;

class Program
{
    public static int donwloadtime = 1;
    private const string executableName = "Downloader.exe";
    private static bool update = true;
    static void Main()
    {
        StartInstaller();
        if (update)
        {
            Thread.Sleep(-1);
        }
    }
    static void StartInstaller()
    {
        Versions versions = Versions.Load();
        ReleasesApiClient apiClient = new();
        List<Task> tasks = new();
        string ytdlp = "null";
        string ffmpeg = "null";
        string downloader = "null";
        try
        {
#pragma warning disable CS8602
#pragma warning disable CS8600
            ytdlp = apiClient.GetLatestReleaseAsync("yt-dlp", "yt-dlp").Result.Name;
            ffmpeg = apiClient.GetLatestReleaseAsync("BtbN", "FFmpeg-Builds").Result.Name;
            downloader = apiClient.GetLatestReleaseAsync("Vader0pr", "Downloader").Result.Name;
#pragma warning restore CS8602
#pragma warning restore CS8600
        }
        catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
        if (ytdlp != versions.Ytdlp) tasks.Add(Task.Run(() => Install(false, "yt-dlp", "yt-dlp", FileType.Exe, 1, new string[] { "yt-dlp.exe" }, Array.Empty<string>())));
        if (ffmpeg != versions.Ffmpeg) tasks.Add(Task.Run(() => Install(false, "BtbN", "FFmpeg-Builds", FileType.Zip, 2, new string[] { "win64", "gpl", "zip" }, new string[] { "shared", "linux" }, true)));
        if (downloader != versions.Downloader) tasks.Add(Task.Run(() => Install(true, "Vader0pr", "Downloader", FileType.Zip, 3, new string[] { "Downloader.zip" }, Array.Empty<string>(), false, true)));
        else
        {
            Process.Start(executableName);
            update = false;
        }
        tasks.ForEach(x => x.Wait());
        versions.Ytdlp = ytdlp ?? "null";
        versions.Ffmpeg = ffmpeg ?? "null";
        versions.Downloader = downloader ?? "null";
        versions.Save();
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Installer finished working");
    }
    static async Task Install(bool run, string owner, string repo, FileType fileType, int downloadId, string[] filter, string[] negativeFilter, bool ffmpeg = false, bool mainFile = false)
    {
        ReleasesApiClient apiClient = new();
        
        Release release = await apiClient.GetLatestReleaseAsync(owner, repo) ?? throw new Exception("No release error");

        List<ReleaseAsset> assets = (release.Assets ?? throw new Exception("No release asset error")).ToList();
        foreach (string item in filter)
        {
            assets = assets.Where(x => (x.Name ?? "").Contains(item)).ToList();
        }
        foreach (string item in negativeFilter)
        {
            assets = assets.Where(x => (x.Name ?? "").Contains(item) == false).ToList();
        }
        ReleaseAsset asset = assets.First();

        string assetName = asset.Name ?? repo + "zip";
        string folderName = assetName.Replace(new FileInfo(assetName).Extension, "");

        if (File.Exists(asset.Name)) File.Delete(asset.Name);
        if (Directory.Exists(folderName)) Directory.Delete(folderName, true);


        using (HttpClient client = new())
        {
            using (FileStream fs = new(assetName, FileMode.Create, FileAccess.Write))
            {
                Stream stream = await client.GetStreamAsync(asset.DownloadUrl);

                donwloadtime = 1;
                int totalBytesRead = 0;
                System.Timers.Timer timer = new(1000);
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
                Console.CursorVisible = false;

                while (fs.Length < asset.Size)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
                    await fs.WriteAsync(buffer.AsMemory(0, bytesRead));
                    totalBytesRead += bytesRead;

                    string progress = "";
                    progress += "Downloading " + asset.Name + " [";
                    for (int i = 0; i < (int)((double)totalBytesRead / (double)asset.Size * 100); i += 10)
                    {
                        progress += "X";
                    }
                    for (int i = 100; i > (int)((double)totalBytesRead / (double)asset.Size * 100); i -= 10)
                    {
                        progress += " ";
                    }
                    progress += "]";
                    progress += $"{totalBytesRead / 1000000}/{asset.Size / 1000000}MB({(int)((double)totalBytesRead / (double)asset.Size * 100)}%) {Math.Round((float)totalBytesRead / 1000000 / donwloadtime, 1)}MB/s {donwloadtime}/{asset.Size / (totalBytesRead / donwloadtime)}s";
                    DownloadProgress.ReportProgress(downloadId, progress);
                }
                timer.Stop();
            }
        }
        if (fileType == FileType.Zip)
        {
            ZipFile.ExtractToDirectory(assetName, folderName);
            File.Delete(assetName);
        }

        if (ffmpeg)
        {
            string path = Path.Combine(assetName.Replace(new FileInfo(assetName).Extension, ""), assetName.Replace(new FileInfo(assetName).Extension, ""), "bin");
            foreach (string file in Directory.GetFiles(path))
            {
                string destination = Path.Combine(Environment.CurrentDirectory + "\\" + new FileInfo(file).Name);
                File.Move(file, destination, true);
            }
            Directory.Delete(assetName.Replace(new FileInfo(assetName).Extension, ""), true);
        }

        if (mainFile)
        {
            File.Move(Directory.GetFiles(Environment.CurrentDirectory + "\\Downloader")[0], Environment.CurrentDirectory + "\\" + "Downloader.exe", true);
            Directory.Delete(assetName.Replace(new FileInfo(assetName).Extension, ""), true);
        }

        if (run) Process.Start(executableName);

        DownloadProgress.ClearProgress(downloadId);
        DownloadProgress.RefreshProgress();
    }
    private enum FileType
    {
        Zip,
        Exe
    }
    private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        donwloadtime++;
        DownloadProgress.RefreshProgress();
    }
}
public static class DownloadProgress
{
    public static Dictionary<int, string> Progresses { get; set; } = new();
    public static void ReportProgress(int id, string progress)
    {
        if (!Progresses.ContainsKey(id))
        {
            Progresses.Add(id, progress);
        }
        else Progresses[id] = progress;
    }
    public static void ClearProgress(int id) => Progresses.Remove(id);
    public static void RefreshProgress()
    {
        Console.Clear();
        foreach (string progress in Progresses.Values)
        {
            Console.WriteLine(progress);
        }
    }
}
public class Versions
{
    private const string VersionsFile = "Versions.json";
    [JsonProperty("downloader")]
    public string Downloader { get; set; } = "";
    [JsonProperty("ffmpeg")]
    public string Ffmpeg { get; set; } = "";
    [JsonProperty("yt-dlp")]
    public string Ytdlp { get; set; } = "";
    public static Versions Load()
    {
        try { return JsonConvert.DeserializeObject<Versions>(File.ReadAllText(VersionsFile)) ?? new Versions(); }
        catch (Exception) { return new Versions(); }
    }
    public void Save() => File.WriteAllText(VersionsFile, JsonConvert.SerializeObject(this));
}