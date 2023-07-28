using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using SpotifyAPI.Web;
using System.Reflection.Emit;
using Newtonsoft.Json;

namespace Downloader
{
    public class Spotify
    {
        private readonly SpotifyClient _spotify;
        public SpotifyClient SpotifyClient => _spotify;
        public Spotify()
        {
            SpotifySettings settings = SpotifySettings.Load();
            var config = SpotifyClientConfig.CreateDefault();

            var request = new ClientCredentialsRequest(settings.ClientId, settings.ClientSecret);
            var response = new OAuthClient(config).RequestToken(request).Result;
            
            _spotify = new SpotifyClient(config.WithToken(response.AccessToken));
        }
    }
    public class SpotifySettings
    {
        private const string saveFileName = "Settings.json";
        public string ClientId { get; set; } = "go to https://developer.spotify.com/dashboard create an app and get the Client ID in https://developer.spotify.com/dashboard/{your app}/settings";
        public string ClientSecret { get; set; } = "go to https://developer.spotify.com/dashboard create an app and get the Client secret in https://developer.spotify.com/dashboard/{your app}/settings";
        private void Save() => File.WriteAllText(saveFileName, JsonConvert.SerializeObject(this, Formatting.Indented));
        internal static SpotifySettings Load() => JsonConvert.DeserializeObject<SpotifySettings>(File.ReadAllText(saveFileName)) ?? new SpotifySettings();
        public static void CheckIfSettingsExist()
        {
            if (!File.Exists(saveFileName)) new SpotifySettings().Save();
        }
    }
}
