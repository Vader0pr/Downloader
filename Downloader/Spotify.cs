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
        SpotifyClient _spotify;
        private SpotifySettings _settings;
        public SpotifyClient SpotifyClient => _spotify;
        public Spotify()
        {
            _settings = SpotifySettings.Load();
            var config = SpotifyClientConfig.CreateDefault();

            var request = new ClientCredentialsRequest(_settings.ClientId, _settings.ClientSecret);
            var response = new OAuthClient(config).RequestToken(request).Result;
            
            _spotify = new SpotifyClient(config.WithToken(response.AccessToken));
        }
        public async Task RefreshToken()
        {
            var config = SpotifyClientConfig
              .CreateDefault()
              .WithAuthenticator(new ClientCredentialsAuthenticator(_settings.ClientId, _settings.ClientSecret));

            _spotify = new SpotifyClient(config);
        }
        private static async Task<string> GenerateRandomString(int length)
        {
            char[] possibleCharacters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
            string result = "";
            for (int i = 0; i < length; i++)
            {
                result += possibleCharacters[new Random().Next(0, possibleCharacters.Length - 1)];
            }
            return result;
        }
    }
    public class SpotifySettings
    {
        private const string saveFileName = "Settings.json";
        public string ClientId { get; set; } = "go to https://developer.spotify.com/dashboard create an app and get the Client ID in https://developer.spotify.com/dashboard/{your app}/settings";
        public string ClientSecret { get; set; } = "go to https://developer.spotify.com/dashboard create an app and get the Client secret in https://developer.spotify.com/dashboard/{your app}/settings";
        private void Save() => File.WriteAllText(saveFileName, JsonConvert.SerializeObject(this, Formatting.Indented));
        internal static SpotifySettings Load() => JsonConvert.DeserializeObject<SpotifySettings>(File.ReadAllText(saveFileName));
        public static void CheckIfSettingsExist()
        {
            if (!File.Exists(saveFileName))
            {
                new SpotifySettings().Save();
            }
        }
    }
}
