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
        bool queueBlocked = false;
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
                string item = AddItemTextbox.Text;
                if (OnlyAudioCheckbox.Checked)
                {
                    switch (QueueItemTypeManager.DetectQueueItemType(AddItemTextbox.Text))
                    {
                        case QueueItemTypeManager.QueueItemTypes.SearchQuery:
                            item += " --extract-audio";
                            break;
                        case QueueItemTypeManager.QueueItemTypes.SpotifyTrack:
                            break;
                        case QueueItemTypeManager.QueueItemTypes.SpotifyAlbum:
                            break;
                        case QueueItemTypeManager.QueueItemTypes.SpotifyPlaylist:
                            break;
                        case QueueItemTypeManager.QueueItemTypes.DownloadLink:
                            item += " --extract-audio";
                            break;
                        default:
                            break;
                    }
                }
                queue.Enqueue(item);
                await UpdateListBox();
            }
        }
        private Task UpdateListBox()
        {
            DownloadQueueListbox.Items.Clear();
            foreach (string item in queue) DownloadQueueListbox.Items.Add(item.Replace('+', ' ').Replace("--extract-audio", "(audio only)"));
            return Task.CompletedTask;
        }
        private async Task KeepListBoxUpToDate()
        {
            while (queue.Count != 0 && DownloadQueueListbox.Items.Count != 0)
            {
                await Task.Delay(3000);
                await Task.Run(UpdateListBox);
            }
        }
        private void DeleteSelectedButton_Click(object sender, EventArgs e)
        {
            List<string> tmpList = queue.ToList();
            queue.Clear();
            foreach (string item in DownloadQueueListbox.SelectedItems)
            {
                bool removed = tmpList.Remove(item.Replace("(audio only)", "--extract-audio"));
                if (!removed) tmpList.Remove(item);
            }
            queue = new(tmpList);
            Task.Run(UpdateListBox);
        }
        private void StartDownloadButton_Click(object sender, EventArgs e) => Task.Run(RunDownloads);
#pragma warning disable CS1998
        private async Task RunDownloads()
        {
            for (int i = 0; i < Math.Clamp(queue.Count, 1, Environment.ProcessorCount); i++)
            {
                try { _ = Task.Run(StartDownload); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
            _ = Task.Run(KeepListBoxUpToDate);
        }
#pragma warning restore CS1998
        private async Task StartDownload()
        {
            while (queueBlocked) await Task.Delay(100);
            queueBlocked = true;
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
                    queue.Dequeue();
                    foreach (SimpleTrack track in album.Tracks.Items)
                    {
                        queue.Enqueue($"{album.Name} {track.Artists.First().Name} - {track.Name}".Trim() + "--extract-audio");
                    }
                    queueBlocked = false;
                    _ = Task.Run(UpdateListBox);
                    _ = Task.Run(RunDownloads);
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
                    queue.Dequeue();
                    foreach (FullTrack track in tracks)
                    {
                        queue.Enqueue($"{track.Artists.First().Name} - {track.Name}".Trim() + "--extract-audio");
                    }
                    queueBlocked = false;
                    _ = Task.Run(UpdateListBox);
                    _ = Task.Run(RunDownloads);
                }
                else MessageBox.Show("Error getting playlist items", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else _ = Task.Run(() => Download(args));
        }
        private async Task Download(string args)
        {
            queue.Dequeue();
            queueBlocked = false;
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
            if (queue.Count > 0) _ = Task.Run(StartDownload);
        }
        private async void AddItemTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ((char)Keys.Return)) await AddItemToQueue();
        }
        private void RefreshListButton_Click(object sender, EventArgs e) => Task.Run(UpdateListBox);

        private void LoadListButton_Click(object sender, EventArgs e) => LoadLists();
        int files = 0;
        int maxFiles = 0;
        private void LoadLists()
        {
            OpenFileDialog dialog = new()
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                InitialDirectory = Environment.CurrentDirectory,
                Multiselect = true,
                Title = "Load list(s)",
                RestoreDirectory = true
            };
            var result = dialog.ShowDialog();
            maxFiles = dialog.FileNames.Length;
            files = 0;
            if (result == DialogResult.OK) foreach (string file in dialog.FileNames) _ = LoadList(file);
            _ = Task.Run(LockListBox);
            dialog.Dispose();
        }
        private async Task LockListBox()
        {
            DownloadQueueListbox.Enabled = false;
            while (files < maxFiles)
            {
                CurrentDownloadInfoTextbox.Text = $"Lists loaded: {files}/{maxFiles}";
                _ = Task.Run(UpdateListBox);
                await Task.Delay(100);
            }
            await Task.Delay(100);
            CurrentDownloadInfoTextbox.Text = $"Lists loaded: {files}/{maxFiles}";
            _ = Task.Run(UpdateListBox);
            DownloadQueueListbox.Enabled = true;
        }
        private async Task LoadList(string file)
        {
            try
            {
                using StreamReader sr = new(file, new FileStreamOptions()
                {
                    Access = FileAccess.Read,
                    Mode = FileMode.Open
                });
                while (!sr.EndOfStream)
                {
                    string? line = await sr.ReadLineAsync();
                    if (!string.IsNullOrWhiteSpace(line)) queue.Enqueue(line);
                }
                files++;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "List loading error"); }
        }
    }
    public class QueueItemTypeManager
    {
        public enum QueueItemTypes
        {
            SearchQuery,
            SpotifyTrack,
            SpotifyAlbum,
            SpotifyPlaylist,
            DownloadLink
        }
        public static QueueItemTypes DetectQueueItemType(string item)
        {
            if (!item.StartsWith("http")) return QueueItemTypes.SearchQuery;
            else if (item.StartsWith("http") && item.Contains("open.spotify.com/track")) return QueueItemTypes.SpotifyTrack;
            else if (item.StartsWith("http") && item.Contains("open.spotify.com/album")) return QueueItemTypes.SpotifyAlbum;
            else if (item.StartsWith("http") && item.Contains("open.spotify.com/playlist")) return QueueItemTypes.SpotifyPlaylist;
            else return QueueItemTypes.DownloadLink;
        }
    }
}