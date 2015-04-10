namespace jla.SpotifyVSExtension.SpotifyAPI.Model
{
    public class ClientVersion
    {
        public Error error { get; set; }
        public int version { get; set; }
        public string client_version { get; set; }
        public bool running { get; set; }
    }
}