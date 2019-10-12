using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Model;
using TrendAudioFromSpotify.UI.Utility;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private List<Audio> likedSongs;

        private ISpotifyServices _spotifyServices;

        private readonly ISettingUtility _settingUtility;

        private readonly IDialogCoordinator _dialogCoordinator;

        private ProgressDialogController progressDialogController;

        #region properties
        private bool _isConnectionEsatblished;
        public bool IsConnectionEsatblished
        {
            get { return _isConnectionEsatblished; }
            set
            {
                _isConnectionEsatblished = value;
                RaisePropertyChanged(nameof(IsConnectionEsatblished));
            }
        }

        private ObservableCollection<Playlist> _playlists;
        public ObservableCollection<Playlist> Playlists
        {
            get { return _playlists; }
            set
            {
                if (value == _playlists) return;
                _playlists = value;
                RaisePropertyChanged(nameof(Playlists));
            }
        }

        private ObservableCollection<Audio> _savedTracks;
        public ObservableCollection<Audio> SavedTracks
        {
            get { return _savedTracks; }
            set
            {
                if (value == _savedTracks) return;
                _savedTracks = value;
                RaisePropertyChanged(nameof(SavedTracks));
            }
        }

        private Playlist _selectedPlaylist;
        public Playlist SelectedPlaylis
        {
            get { return _selectedPlaylist; }
            set
            {
                if (value == _selectedPlaylist) return;
                _selectedPlaylist = value;
                RaisePropertyChanged(nameof(SelectedPlaylis));

                if (_selectedPlaylist != null)
                    GetPlaylistsAudios();
            }
        }

        #endregion

        public MainViewModel()
        {
            _dialogCoordinator = DialogCoordinator.Instance;
            _settingUtility = new SettingUtility();
        }

        #region dialogs
        private async Task ShowConnectingMessage()
        {
            progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Loading", "Connecting to Spotify...");
            progressDialogController.SetIndeterminate();

            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        private async Task ShowMessage(string header, string message)
        {
            await _dialogCoordinator.ShowMessageAsync(this, header, message);
        }

        private async Task HideConnectingMessage()
        {
            try
            {
                if (progressDialogController != null)
                    await progressDialogController.CloseAsync();
            }
            catch { }
        }
        #endregion

        #region private auth methods
        private async Task EstablishConnection()
        {
            var accessToken = _settingUtility.GetAccessToken();

            await ShowConnectingMessage();

            SpotifyProvider.Authorization.AuthReceived += OnAuthResponse;

            if (accessToken != null)
            {
                await AuthByToken(accessToken);

                if (IsConnectionEsatblished)
                    await FetchData();
            }
            else
            {
                await Task.Run(() => SpotifyProvider.Auth());
            }
        }

        private async Task AuthByToken(Setting accessToken)
        {
            SpotifyWebAPI api = new SpotifyWebAPI
            {
                AccessToken = accessToken.Value,
                TokenType = "Bearer"
            };

            _spotifyServices = new SpotifyServices(api);

            if (_spotifyServices.PrivateProfile.StatusCode() == HttpStatusCode.OK)
            {
                IsConnectionEsatblished = true;

                await ShowMessage("Notification", "Succesfully authorized in Spotify!");

                await HideConnectingMessage();
            }
            else
            {
                await Task.Run(() => SpotifyProvider.Auth());
            }
        }

        private async void OnAuthResponse(object sender, AuthorizationCode payload)
        {
            if (sender is AuthorizationCodeAuth authorization)
            {
                authorization.Stop();

                Token token = await authorization.ExchangeCode(payload.Code);

                SpotifyWebAPI api = new SpotifyWebAPI
                {
                    AccessToken = token.AccessToken,
                    TokenType = token.TokenType
                };

                _spotifyServices = new SpotifyServices(api);

                _settingUtility.SaveAccessToken(api.AccessToken);

                IsConnectionEsatblished = true;

                await HideConnectingMessage();
                await ShowMessage("Notification", "Succesfully authorized in Spotify!");

                await FetchData();
            }
            else
            {
                await HideConnectingMessage();
                await ShowMessage("Notification", "Cant authorize to Spotify by give  credentials.");

                throw new Exception();
            }

            await HideConnectingMessage();
        }
        #endregion

        #region private data metods
        private async Task FetchData()
        {
            var playlists = (await _spotifyServices.GetAllPlaylists()).Select(x => new Playlist(x)).ToList();

            Playlists = new ObservableCollection<Playlist>(playlists);

            likedSongs = (await _spotifyServices.GetSongs(50)).Select(x => new Audio(x.Track)).ToList();

            SavedTracks = new ObservableCollection<Audio>(likedSongs);
        }

        private async void GetPlaylistsAudios()
        {
            var audios = await _spotifyServices.GetPlaylistSongs(_selectedPlaylist.SimplePlaylist.Id);

            SavedTracks = new ObservableCollection<Audio>(audios.Select(x => new Audio(x.Track)));
        }
        #endregion

        #region commands
        private RelayCommand _connectToSpotifyCommand;
        public RelayCommand ConnectToSpotifyCommand => _connectToSpotifyCommand ?? (_connectToSpotifyCommand = new RelayCommand(ConnectToSpotify));
        private async void ConnectToSpotify()
        {
            await EstablishConnection();
        }

        private RelayCommand _likedSongsLoadCommand;
        public RelayCommand LikedSongsLoadCommand => _likedSongsLoadCommand ?? (_likedSongsLoadCommand = new RelayCommand(LikedSongsLoad));
        private void LikedSongsLoad()
        {
            SavedTracks = new ObservableCollection<Audio>(likedSongs);

            SelectedPlaylis = null;
        }
        #endregion
    }
}