using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Windows;
using jla.SpotifyVSExtension.SpotifyAPI.Model;
using Newtonsoft.Json;

namespace jla.SpotifyVSExtension.SpotifyAPI
{
    public class SpotifyAPI
    {
        private const string Host = "127.0.0.1";
        private readonly string _oauth;
        private readonly WebClient _webClient;
        private string _cfid;

        private int TimeStamp
        {
            get
            {
                return Convert.ToInt32((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
            }
        }

        public SpotifyAPI()
        {
            _oauth = GetOAuth();

            _webClient = new WebClient();

            _webClient.Headers.Add("Origin", "https://embed.spotify.com");
            _webClient.Headers.Add("Referer", "https://embed.spotify.com/?uri=spotify:track:5Zp4SWOpbuOdnsxLqwgutt");
        }

        public CFID GenerateCfid()
        {
            var a = SendRequest("simplecsrf/token.json");
            var d = (List<CFID>)JsonConvert.DeserializeObject(a, typeof(List<CFID>));
            _cfid = d[0].token;
            return d[0];
        }

        public Status Play(string uri)
        {
            var a = SendRequest("remote/play.json?uri=" + uri, true, true, -1);
            var d = (List<Status>)JsonConvert.DeserializeObject(a, typeof(List<Status>));
            return d[0];
        }

        public Status Resume()
        {
            var a = SendRequest("remote/pause.json?pause=false", true, true, -1);
            var d = (List<Status>)JsonConvert.DeserializeObject(a, typeof(List<Status>));
            return d[0];
        }

        public Status Pause()
        {
            var a = SendRequest("remote/pause.json?pause=true", true, true, -1);
            var d = (List<Status>)JsonConvert.DeserializeObject(a, typeof(List<Status>));
            return d[0];
        }

        public Status GetCurrentStatus(int wait = -1)
        {
            var a = SendRequest("remote/status.json", true, true, wait);
            var d = (List<Status>)JsonConvert.DeserializeObject(a, typeof(List<Status>));
            return d[0];
        }

        public ClientVersion GetClientVersion()
        {
            var a = SendRequest("service/version.json?service=remote");
            var d = (List<ClientVersion>)JsonConvert.DeserializeObject(a, typeof(List<ClientVersion>));
            return d[0];
        }

        public string GetAlbumCover(string uri)
        {
            try
            {
                var raw = new WebClient().DownloadString("http://open.spotify.com/album/" + uri.Split(new[] { ":" }, StringSplitOptions.None)[2]);
                const string metaPropertyOgImage = "<meta property=\"og:image\" content=\"";
                var startIndex = raw.IndexOf(metaPropertyOgImage) + metaPropertyOgImage.Length;
                var endIndex = raw.IndexOf("\"", startIndex);
                var url = raw.Substring(startIndex, endIndex - startIndex);
                return url;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error getting album cover: " + e.Message, "Spotify Player");
                return String.Empty;
            }
            return String.Empty;
        }

        private string SendRequest(string request)
        {
            return SendRequest(request, false, false, -1);
        }

        private string SendRequest(string request, bool oauth, bool cfid)
        {
            return SendRequest(request, oauth, cfid, -1);
        }

        private string SendRequest(string request, bool oauth, bool cfid, int wait)
        {
            var parameters = "?&ref=&cors=&_=" + TimeStamp;
            if (request.Contains("?"))
            {
                parameters = parameters.Substring(1);
            }

            if (oauth)
            {
                parameters += "&oauth=" + _oauth;
            }
            if (cfid)
            {
                parameters += "&csrf=" + _cfid;
            }

            if (wait != -1)
            {
                parameters += "&returnafter=" + wait;
                parameters += "&returnon=login%2Clogout%2Cplay%2Cpause%2Cerror%2Cap";
            }

            var a = "http://" + Host + ":4380/" + request + parameters;
            string derp;
            try
            {
                derp = _webClient.DownloadString(a);
                derp = "[ " + derp + " ]";
            }
            catch (Exception z)
            {
                if (Process.GetProcessesByName("SpotifyWebHelper").Length < 1)
                {
                    try
                    {
                        Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Spotify\\Data\\SpotifyWebHelper.exe");
                    }
                    catch (Exception dd)
                    {
                        throw new Exception("Could not launch SpotifyWebHelper. Your installation of Spotify might be corrupt or you might not have Spotify installed", dd);
                    }

                    return SendRequest(request, oauth, cfid);
                }
                //spotifywebhelper is running but we still can't connect, wtf?!
                throw new Exception("Unable to connect to SpotifyWebHelper", z);
            }
            return derp;
        }

        private string GetOAuth()
        {
            var raw = new WebClient().DownloadString("https://open.spotify.com/token");
            var d = (Token)JsonConvert.DeserializeObject(raw, typeof(Token));
            return d.t;
        }
    }
}