using GitHubAPIWrapper.Releases;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Timers;

class Program
{
    public static int donwloadtime = 1;
    static void Main()
    {
        StartInstaller();
        Thread.Sleep(-1);
    }
    static async void StartInstaller()
    {
        await Task.Run(() => Install(false, "yt-dlp", "yt-dlp", FileType.Exe, new string[] { "yt-dlp.exe" }, new string[] { }));
        await Task.Run(() => Install(false, "BtbN", "FFmpeg-Builds", FileType.Zip, new string[] { "win64", "gpl", "zip" }, new string[] { "shared", "linux" }, true));
        await Task.Run(() => Install(true, "Vader0pr", "Downloader", FileType.Zip, new string[] { "Downloader.zip" }, new string[] { }, false, true));
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Installer finished working");
    }
    static async Task Install(bool run, string owner, string repo, FileType fileType, string[] filter, string[] negativeFilter, bool ffmpeg = false, bool mainFile = false, string executableName = "Downloader.exe")
    {
        ReleasesApiClient apiClient = new();
        
        Release? release = await apiClient.GetLatestReleaseAsync(owner, repo);

        List<ReleaseAsset> assets = release.Assets.ToList();
        foreach (string item in filter)
        {
            assets = assets.Where(x => x.Name.Contains(item)).ToList();
        }
        foreach (string item in negativeFilter)
        {
            assets = assets.Where(x => x.Name.Contains(item) == false).ToList();
        }
        ReleaseAsset asset = assets.First();

        Console.WriteLine("Latest release: " + release.Name);

        string folderName = asset.Name.Replace(new FileInfo(asset.Name).Extension, "");

        if (File.Exists(asset.Name)) File.Delete(asset.Name);
        if (Directory.Exists(folderName)) Directory.Delete(folderName, true);

        Console.WriteLine($"Downloading: {asset.Name}(from {asset.DownloadUrl})...");

        using (HttpClient client = new())
        {
            using (FileStream fs = new(asset.Name, FileMode.Create, FileAccess.Write))
            {
                Stream stream = await client.GetStreamAsync(asset.DownloadUrl);

                donwloadtime = 1;
                Console.WriteLine("Downloading...");
                int totalBytesRead = 0;
                System.Timers.Timer timer = new(1000);
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
                Console.Clear();
                Console.CursorVisible = false;
                while (fs.Length < asset.Size)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    fs.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;
                    Console.SetCursorPosition(1, 1);
                    Console.Write("Downloading " + asset.Name + " [");
                    for (int i = 0; i < (int)((double)totalBytesRead / (double)asset.Size * 100); i += 10)
                    {
                        Console.Write("X");
                    }
                    for (int i = 100; i > (int)((double)totalBytesRead / (double)asset.Size * 100); i -= 10)
                    {
                        Console.Write(" ");
                    }
                    Console.Write("]");
                    Console.Write($"{totalBytesRead / 1000000}/{asset.Size / 1000000}MB({(int)((double)totalBytesRead / (double)asset.Size * 100)}%) {Math.Round((float)totalBytesRead / 1000000 / donwloadtime, 1)}MB/s {donwloadtime}/{asset.Size / (totalBytesRead / donwloadtime)}s       ");
                    Console.SetCursorPosition(1, 1);
                }
                Console.Clear();
                timer.Stop();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Data downloaded");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        if (fileType == FileType.Zip)
        {
            Console.WriteLine("Extracting data...");
            ZipFile.ExtractToDirectory(asset.Name, folderName);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Data extracted");
            Console.ForegroundColor = ConsoleColor.White;
            File.Delete(asset.Name);
        }

        if (ffmpeg)
        {
            string path = Path.Combine(asset.Name.Replace(new FileInfo(asset.Name).Extension, ""), asset.Name.Replace(new FileInfo(asset.Name).Extension, ""), "bin");
            foreach (string file in Directory.GetFiles(path))
            {
                string destination = Path.Combine(Environment.CurrentDirectory + "\\" + new FileInfo(file).Name);
                File.Move(file, destination, true);
            }
            Directory.Delete(asset.Name.Replace(new FileInfo(asset.Name).Extension, ""), true);
        }

        if (mainFile)
        {
            File.Move(Directory.GetFiles(Environment.CurrentDirectory + "\\Downloader")[0], Environment.CurrentDirectory + "\\" + "Downloader.exe", true);
            Directory.Delete(asset.Name.Replace(new FileInfo(asset.Name).Extension, ""), true);
        }

        if (run)
        {
            Console.WriteLine("Executing program...");
            Process.Start(executableName);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Program executed");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
    private enum FileType
    {
        Zip,
        Exe
    }
    private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        donwloadtime++;
    }
}