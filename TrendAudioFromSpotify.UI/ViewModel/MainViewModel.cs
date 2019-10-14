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
        private bool _isSpotifyCredsEntered;
        public bool IsSpotifyCredsEntered
        {
            get { return _isSpotifyCredsEntered;  }
            set
            {
                if (value == _isSpotifyCredsEntered) return;
                _isSpotifyCredsEntered = value;
                RaisePropertyChanged(nameof(IsSpotifyCredsEntered));
            }
        }


        private string _userId;
        public string UserId
        {
            get { return _userId; }
            set
            {
                if (value == _userId) return;
                _userId = value;
                RaisePropertyChanged(nameof(UserId));
            }
        }

        private string _secretId;
        public string SecretId
        {
            get { return _secretId; }
            set
            {
                if (value == _secretId) return;
                _secretId = value;
                RaisePropertyChanged(nameof(SecretId));
            }
        }

        private string _redirectUri;
        public string RedirectUri
        {
            get { return _redirectUri; }
            set
            {
                if (value == _redirectUri) return;
                _redirectUri = value;
                RaisePropertyChanged(nameof(RedirectUri));
            }
        }

        private string _serverUri;
        public string ServerUri
        {
            get { return _serverUri; }
            set
            {
                if (value == _serverUri) return;
                _serverUri = value;
                RaisePropertyChanged(nameof(ServerUri));
            }
        }

        private bool _isProcessingAreaIsBusy;
        public bool IsProcessingAreaIsBusy
        {
            get { return _isProcessingAreaIsBusy; }
            set
            {
                if (value == _isProcessingAreaIsBusy) return;
                _isProcessingAreaIsBusy = value;
                RaisePropertyChanged(nameof(IsProcessingAreaIsBusy));
            }
        }

        private int _appearsAtLeastInX;
        public int AppearsAtLeastInX
        {
            get { return _appearsAtLeastInX; }
            set
            {
                if (value == _appearsAtLeastInX) return;
                _appearsAtLeastInX = value;
                RaisePropertyChanged(nameof(AppearsAtLeastInX));
            }
        }

        private bool _isSongsAreaBusy;
        public bool IsSongsAreaBusy
        {
            get { return _isSongsAreaBusy; }
            set
            {
                if (value == _isSongsAreaBusy) return;
                _isSongsAreaBusy = value;
                RaisePropertyChanged(nameof(IsSongsAreaBusy));
            }
        }

        private bool _isPlaylistsAreaBusy;
        public bool IsPlaylistsAreaBusy
        {
            get { return _isPlaylistsAreaBusy; }
            set
            {
                if (value == _isPlaylistsAreaBusy) return;
                _isPlaylistsAreaBusy = value;
                RaisePropertyChanged(nameof(IsPlaylistsAreaBusy));
            }
        }

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

        private ObservableCollection<Audio> _trendTracks;
        public ObservableCollection<Audio> TrendTracks
        {
            get { return _trendTracks; }
            set
            {
                if (value == _trendTracks) return;
                _trendTracks = value;
                RaisePropertyChanged(nameof(TrendTracks));
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
            IsSpotifyCredsEntered = LoadSettings();
            if (IsSpotifyCredsEntered)
                SpotifyProvider.InitProvider(_userId, _secretId, _redirectUri, _serverUri);
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
        private bool LoadSettings()
        {
            try
            {
                UserId = _settingUtility.GetByKey(nameof(UserId)).Value;
                SecretId = _settingUtility.GetByKey(nameof(SecretId)).Value;
                RedirectUri = _settingUtility.GetByKey(nameof(RedirectUri)).Value;
                ServerUri = _settingUtility.GetByKey(nameof(ServerUri)).Value;
                return true;
            }
            catch
            {
                return false;
            }
        }

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
            IsSongsAreaBusy = IsPlaylistsAreaBusy = true;

            var playlists = (await _spotifyServices.GetAllPlaylists()).Select(x => new Playlist(x)).ToList();

            Playlists = new ObservableCollection<Playlist>(playlists);

            IsPlaylistsAreaBusy = false;

            likedSongs = (await _spotifyServices.GetSongs()).Select(x => new Audio(x.Track)).ToList();

            SavedTracks = new ObservableCollection<Audio>(likedSongs);

            IsSongsAreaBusy = false;
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
            IsSongsAreaBusy = true;

            SavedTracks = new ObservableCollection<Audio>(likedSongs);

            SelectedPlaylis = null;

            IsSongsAreaBusy = false;
        }

        private RelayCommand _getTrendsCommand;
        public RelayCommand GetTrendsCommand => _getTrendsCommand ?? (_getTrendsCommand = new RelayCommand(GetTrends));
        private void GetTrends()
        {
            Task.Run(async () =>
            {
                try
                {
                    IsProcessingAreaIsBusy = true;

                    var selectedAudios = SavedTracks.Where(x => x.IsChecked).ToList();

                    var selectedPlaylists = Playlists.Where(x => x.IsChecked).Select(x => x.SimplePlaylist.Id).ToList();

                    var audiosOfPlaylists = new Dictionary<string, List<FullTrack>>();

                    foreach (var selectedPlaylist in selectedPlaylists)
                    {
                        var audiosOfPlaylist = (await _spotifyServices.GetPlaylistSongs(selectedPlaylist)).Select(x => x.Track).ToList();

                        audiosOfPlaylists.Add(selectedPlaylist, audiosOfPlaylist);
                    }

                    var trendAudios = new Dictionary<Audio, int>();

                    foreach (var selectedAudio in selectedAudios)
                    {
                        int counter = 0;

                        foreach (var audiosOfPlaylist in audiosOfPlaylists)
                        {
                            var targetTrack = audiosOfPlaylist.Value.FirstOrDefault(x => x.Id == selectedAudio.Track.Id);

                            if (targetTrack != null)
                                counter++;
                        }

                        if (counter >= _appearsAtLeastInX)
                            trendAudios.Add(selectedAudio, counter);
                    }

                    TrendTracks = new ObservableCollection<Audio>(trendAudios.Select(x => new Audio(x.Key.Track) { Hits = x.Value }).ToList());
                }
                finally
                {
                    IsProcessingAreaIsBusy = false;
                }
            });
        }

        private RelayCommand _saveSpotifyCredentialsCommand;
        public RelayCommand SaveSpotifyCredentialsCommand => _saveSpotifyCredentialsCommand ?? (_saveSpotifyCredentialsCommand = new RelayCommand(SaveSpotifyCredentials));
        private void SaveSpotifyCredentials()
        {
            try
            {
                string userId = _userId;
                string secretId = _secretId;
                string redirectUri = _redirectUri;
                string serverUri = _serverUri;

                _settingUtility.Save(new Setting(nameof(UserId), userId));
                _settingUtility.Save(new Setting(nameof(SecretId), secretId));
                _settingUtility.Save(new Setting(nameof(RedirectUri), redirectUri));
                _settingUtility.Save(new Setting(nameof(ServerUri), serverUri));

                SpotifyProvider.InitProvider(_userId, _secretId, _redirectUri, _serverUri);

                IsSpotifyCredsEntered = true;
            }
            catch
            {
                IsSpotifyCredsEntered = true;
            }
        }
        #endregion
    }
}