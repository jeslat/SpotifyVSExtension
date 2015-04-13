using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
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

            _webClient = new WebClient {Encoding = Encoding.UTF8};
            _webClient.Headers.Add("Origin", "https://embed.spotify.com");
            _webClient.Headers.Add("Referer", "https://embed.spotify.com/?uri=spotify:track:5Zp4SWOpbuOdnsxLqwgutt");
        }

        public CFID GenerateCfid()
        {
            var response = SendRequest("simplecsrf/token.json");
            var cfid = ((List<CFID>)JsonConvert.DeserializeObject(response, typeof(List<CFID>)))[0];
            _cfid = cfid.token;
            return cfid;
        }

        public Status Play(string uri)
        {
            var response = SendRequest("remote/play.json?uri=" + uri, true, true, -1);
            var status = ((List<Status>)JsonConvert.DeserializeObject(response, typeof(List<Status>)))[0];
            return status;
        }

        public Status Resume()
        {
            var response = SendRequest("remote/pause.json?pause=false", true, true, -1);
            var status = ((List<Status>)JsonConvert.DeserializeObject(response, typeof(List<Status>)))[0];
            return status;
        }

        public Status Pause()
        {
            var response = SendRequest("remote/pause.json?pause=true", true, true, -1);
            var status = ((List<Status>)JsonConvert.DeserializeObject(response, typeof(List<Status>)))[0];
            return status;
        }

        public Status GetCurrentStatus(int wait = -1)
        {
            var response = SendRequest("remote/status.json", true, true, wait);
            var status = ((List<Status>)JsonConvert.DeserializeObject(response, typeof(List<Status>)))[0];
            return status;
        }

        public ClientVersion GetClientVersion()
        {
            var response = SendRequest("service/version.json?service=remote");
            var clientVersion = ((List<ClientVersion>)JsonConvert.DeserializeObject(response, typeof(List<ClientVersion>)))[0];
            return clientVersion;
        }

        public string GetAlbumCover(string uri)
        {
            try
            {
                var response = new WebClient().DownloadString("http://open.spotify.com/album/" + uri.Split(new[] { ":" }, StringSplitOptions.None)[2]);
                const string metaPropertyOgImage = "<meta property=\"og:image\" content=\"";
                var startIndex = response.IndexOf(metaPropertyOgImage) + metaPropertyOgImage.Length;
                var endIndex = response.IndexOf("\"", startIndex);
                var url = response.Substring(startIndex, endIndex - startIndex);
                return url;
            }
            catch (Exception e)
            {
                return String.Empty;
            }
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
            string response;
            try
            {
                response = _webClient.DownloadString(a);
                response = "[ " + response + " ]";
            }
            catch (Exception e)
            {
                if (Process.GetProcessesByName("SpotifyWebHelper").Length < 1)
                {
                    try
                    {
                        Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Spotify\\SpotifyWebHelper.exe");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Could not launch SpotifyWebHelper", ex);
                    }

                    return SendRequest(request, oauth, cfid);
                }
                throw new Exception("Unable to connect to SpotifyWebHelper", e);
            }
            return response;
        }

        private string GetOAuth()
        {
            var response = new WebClient().DownloadString("https://open.spotify.com/token");
            var token = ((Token)JsonConvert.DeserializeObject(response, typeof(Token))).t;
            return token;
        }
    }
}