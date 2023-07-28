using SpotifyAPI;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Downloader
{
    public partial class MainForm : Form
    {
        Queue<string> queue = new();
        public MainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            SpotifySettings.CheckIfSettingsExist();
        }
        private async void AddToQueueButton_Click(object sender, EventArgs e) => await AddItemToQueue();
        private async Task AddItemToQueue()
        {
            if (!string.IsNullOrWhiteSpace(AddItemTextbox.Text))
            {
                queue.Enqueue(AddItemTextbox.Text);
                await UpdateListBox();
            }
        }
        private Task UpdateListBox()
        {
            DownloadQueueListbox.Items.Clear();
            foreach (string item in queue) DownloadQueueListbox.Items.Add(item.Replace('+', ' ').Replace("--extract-audio", "(audio only)"));
            return Task.CompletedTask;
        }
        private void DeleteSelectedButton_Click(object sender, EventArgs e)
        {
            List<string> tmpList = queue.ToList();
            foreach (string item in DownloadQueueListbox.SelectedItems) tmpList.Remove(item);
            queue = new(tmpList);
            Task.Run(UpdateListBox);
        }
        private void StartDownloadButton_Click(object sender, EventArgs e)
        {
            try { Task.Run(StartDownload); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        private async Task StartDownload()
        {
            string args = queue.Peek();
            if (args.Length > 60) CurrentDownloadLabel.Text = args.Remove(60).Replace('+', ' ').Replace("--extract-audio", "(audio only)") + "...";
            else CurrentDownloadLabel.Text = args.Replace('+', ' ').Replace("--extract-audio", "(audio only)");
            if (!args.StartsWith("http"))
            {
                args = ("ytsearch:" + args).Trim().Replace(' ', '+').Replace("--extract-audio", " --extract-audio");
                _ = Task.Run(() => Download(args));
            }
            else if (args.StartsWith("http") && args.Contains("open.spotify.com/track"))
            {
                UriBuilder uri = new(args);
                Spotify spotify = new();
                FullTrack track = await spotify.SpotifyClient.Tracks.Get(uri.Path.Split('/').Last());
                args = $"ytsearch:{track.Album.Name} {track.Artists.First().Name} - {track.Name}".Trim().Replace(' ', '+');
                _ = Task.Run(() => Download(args));
            }
            else if (args.StartsWith("http") && args.Contains("open.spotify.com/album"))
            {
                UriBuilder uri = new(args);
                Spotify spotify = new();
                FullAlbum album = await spotify.SpotifyClient.Albums.Get(uri.Path.Split('/').Last());
                if (album.Tracks.Items is not null)
                {
                    foreach (SimpleTrack track in album.Tracks.Items)
                    {
                        queue.Enqueue($"{album.Name} {track.Artists.First().Name} - {track.Name}".Trim() + "--extract-audio");
                    }
                    queue.Dequeue();
                    _ = Task.Run(UpdateListBox);
                    _ = Task.Run(StartDownload);
                }
                else MessageBox.Show("Error getting album items", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (args.StartsWith("http") && args.Contains("open.spotify.com/playlist"))
            {
                UriBuilder uri = new(args);
                Spotify spotify = new();
                FullPlaylist playlist = await spotify.SpotifyClient.Playlists.Get(uri.Path.Split('/').Last());
                if (playlist.Tracks is not null && playlist.Tracks.Items is not null)
                {
                    List<FullTrack> tracks = new();
                    playlist.Tracks.Items.ForEach(x => tracks.Add((FullTrack)x.Track));
                    foreach (FullTrack track in tracks)
                    {
                        queue.Enqueue($"{track.Artists.First().Name} - {track.Name}".Trim() + "--extract-audio");
                    }
                    queue.Dequeue();
                    _ = Task.Run(UpdateListBox);
                    _ = Task.Run(StartDownload);
                }
                else MessageBox.Show("Error getting playlist items", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else _ = Task.Run(() => Download(args));
        }
        private async Task Download(string args)
        {
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Downloads"))) Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Downloads"));
            Process downloadProcess = new()
            {
                StartInfo =
                new ProcessStartInfo
                {
                    UseShellExecute = false,
                    FileName = "yt-dlp.exe",
                    Arguments = args,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Path.Combine(Environment.CurrentDirectory, "Downloads")
                }
            };
            downloadProcess.Start();
            while (!downloadProcess.HasExited) CurrentDownloadInfoTextbox.Text = await downloadProcess.StandardOutput.ReadLineAsync();
            queue.Dequeue();
            _ = Task.Run(UpdateListBox);
            if (queue.Count > 0) _ = Task.Run(StartDownload);
        }
        private async void AddItemTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ((char)Keys.Return)) await AddItemToQueue();
        }
    }
}