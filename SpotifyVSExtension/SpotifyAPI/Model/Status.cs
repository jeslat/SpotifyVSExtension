namespace jla.SpotifyVSExtension.SpotifyAPI.Model
{
    public class Status
    {
        public Error error { get; set; }
        public int version { get; set; }
        public string client_version { get; set; }
        public bool playing { get; set; }
        public bool shuffle { get; set; }
        public bool repeat { get; set; }
        public bool play_enabled { get; set; }
        public bool prev_enabled { get; set; }
        public Track track { get; set; }
        public double playing_position { get; set; }
        public int server_time { get; set; }
        public double volume { get; set; }
        public bool online { get; set; }
        public Open_graph_state open_graph_state { get; set; }
        public bool running { get; set; }
    }
}