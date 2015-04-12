using System;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;
using WindowsInput;
using WindowsInput.Native;
using jla.SpotifyVSExtension.SpotifyAPI.Model;

namespace jla.SpotifyVSExtension
{
    public partial class MyControl
    {
        private readonly SpotifyAPI.SpotifyAPI _spotifyApi;
        private Status _currentStatus;
        private Timer _timer;

        public MyControl()
        {
            InitializeComponent();

            _spotifyApi = new SpotifyAPI.SpotifyAPI();
            var cfid = _spotifyApi.GenerateCfid();
            if (cfid.error == null)
            {
                RefreshCurrentStatus();
                SetTimer();
            }
            else
            {
                TrackName.Text = "Unable connect to Spotify";
            }
        }

        private void SetTimer()
        {
            _timer = new Timer();
            _timer.Elapsed += OnTimedEvent;
            _timer.Interval = 1000;
            _timer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            RefreshCurrentStatus();
        }

        private void Error()
        {
            _timer.Stop();
            Dispatcher.Invoke(() =>
            {
                TrackName.Text = "Unable connect to Spotify";
                ArtistName.Text = String.Empty;
                AlbumCover.Source = new BitmapImage(new Uri("http://127.0.0.1", UriKind.Absolute));
            });
        }

        private void RefreshCurrentStatus()
        {
            var previousStatus = _currentStatus;
            try
            {
                _currentStatus = _spotifyApi.GetCurrentStatus();
            }
            catch (Exception e)
            {
                Error();
            }
            if (_currentStatus.error != null)
            {
                Error();
            }

            if (previousStatus != null && previousStatus.Equals(_currentStatus))
                return;
            if (_currentStatus != null && _currentStatus.error == null)
            {
                if (_currentStatus.track != null)
                {
                    var albumCover = _spotifyApi.GetAlbumCover(_currentStatus.track.album_resource.uri);
                    var albumCoverUri = String.IsNullOrEmpty(albumCover) ? new Uri("http://127.0.0.1", UriKind.Absolute) : new Uri(albumCover, UriKind.Absolute);

                    Dispatcher.Invoke(() =>
                    {
                        if (_currentStatus.playing)
                        {
                            SetPauseIcon();
                        }
                        else
                        {
                            SetPlayIcon();
                        }
                        TrackName.Text = _currentStatus.track.track_resource.name;
                        ArtistName.Text = _currentStatus.track.artist_resource.name;
                        AlbumCover.Source = new BitmapImage(albumCoverUri);
                    });
                }
                else
                {
                    Dispatcher.Invoke(SetPlayIcon);
                }
            }
        }

        private void PlayPauseClick(object sender, RoutedEventArgs e)
        {
            if (_currentStatus.playing)
            {
                _currentStatus = _spotifyApi.Pause();
                SetPlayIcon();
            }
            else
            {
                _currentStatus = _spotifyApi.Resume();
                SetPauseIcon();
            }
        }

        private void SetPauseIcon()
        {
            PlayPauseButton.Content = "\uE103";
        }

        private void SetPlayIcon()
        {
            PlayPauseButton.Content = "\uE102";
        }

        private void NextTrackClick(object sender, RoutedEventArgs e)
        {
            var inputSimulator = new InputSimulator();
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.MEDIA_NEXT_TRACK);
        }

        private void PreviousTrackClick(object sender, RoutedEventArgs e)
        {
            var inputSimulator = new InputSimulator();
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.MEDIA_PREV_TRACK);
        }

        private void MuteClick(object sender, RoutedEventArgs e)
        {
            var inputSimulator = new InputSimulator();
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VOLUME_MUTE);
        }

        private void VolumeUpClick(object sender, RoutedEventArgs e)
        {
            var inputSimulator = new InputSimulator();
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VOLUME_UP);
        }

        private void VolumeDownClick(object sender, RoutedEventArgs e)
        {
            var inputSimulator = new InputSimulator();
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VOLUME_DOWN);
        }
    }
}