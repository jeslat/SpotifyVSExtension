namespace jla.SpotifyVSExtension.SpotifyAPI.Model
{
    public class Track
    {
        public Resource track_resource { get; set; }
        public Resource artist_resource { get; set; }
        public Resource album_resource { get; set; }
        public int length { get; set; }
        public string track_type { get; set; }
    }
}