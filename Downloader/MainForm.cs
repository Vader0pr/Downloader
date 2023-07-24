using SpotifyAPI.Web;
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

        private void button1_Click(object sender, EventArgs e)
        {
            queue.Enqueue(textBox1.Text);
            Task.Run(UpdateListBox);
        }
        private Task UpdateListBox()
        {
            listBox1.Items.Clear();
            foreach (string item in queue) listBox1.Items.Add(item.Replace('+', ' ').Replace("--extract-audio", "(audio only)"));
            return Task.CompletedTask;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<string> tmpList = queue.ToList();
            foreach (string item in listBox1.SelectedItems) tmpList.Remove(item);
            queue = new(tmpList);
            Task.Run(UpdateListBox);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try { Task.Run(StartDownload); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async Task StartDownload()
        {
            string args = queue.Peek();
            if (args.Length > 60) label1.Text = args.Remove(60).Replace('+', ' ').Replace("--extract-audio", "(audio only)") + "...";
            else label1.Text = args.Replace('+', ' ').Replace("--extract-audio", "(audio only)");
            if (!args.StartsWith("http"))
            {
                args = ("ytsearch:" + args).Trim().Replace(' ', '+').Replace("--extract-audio", " --extract-audio");
                Download(args);
            }
            else if (args.StartsWith("http") && args.Contains("open.spotify.com/track"))
            {
                UriBuilder uri = new(args);
                Spotify spotify = new();
                FullTrack track = await spotify.SpotifyClient.Tracks.Get(uri.Path.Split('/').Last());
                args = ($"ytsearch:{track.Album.Name} {track.Artists.FirstOrDefault().Name} - {track.Name}").Trim().Replace(' ', '+');
                Download(args);
            }
            else if (args.StartsWith("http") && args.Contains("open.spotify.com/album"))
            {
                UriBuilder uri = new(args);
                Spotify spotify = new();
                FullAlbum album = await spotify.SpotifyClient.Albums.Get(uri.Path.Split('/').Last());
                foreach (SimpleTrack track in album.Tracks.Items)
                {
                    queue.Enqueue(($"{album.Name} {track.Artists.FirstOrDefault().Name} - {track.Name}").Trim() + "--extract-audio");
                }
                queue.Dequeue();
                Task.Run(UpdateListBox);
                Task.Run(StartDownload);
            }
        }
        private async Task Download(string args)
        {
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Downloads"))) Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Downloads"));
            Process downloadProcess = new();
            downloadProcess.StartInfo =
                new ProcessStartInfo
                {
                    UseShellExecute = false,
                    FileName = "yt-dlp.exe",
                    Arguments = args,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Path.Combine(Environment.CurrentDirectory, "Downloads")
                };
            downloadProcess.Start();
            while (!downloadProcess.HasExited) textBox2.Text = await downloadProcess.StandardOutput.ReadLineAsync();
            queue.Dequeue();
            Task.Run(UpdateListBox);
            if (queue.Count > 0) Task.Run(StartDownload);
        }
    }
}
