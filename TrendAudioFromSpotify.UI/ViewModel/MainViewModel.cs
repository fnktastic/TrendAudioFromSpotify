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
using System.Windows.Media;
using TrendAudioFromSpotify.Data.Repository;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Model;
using TrendAudioFromSpotify.UI.Utility;
using DbContext = TrendAudioFromSpotify.Data.DataAccess.Context;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region fields
        private List<Audio> likedSongs;

        private ISpotifyServices _spotifyServices;

        private readonly ISettingUtility _settingUtility;

        private readonly IDialogCoordinator _dialogCoordinator;

        private ProgressDialogController progressDialogController;


        private readonly DbContext _context;

        private readonly IAudioRepository _audioRepository;
        #endregion

        #region properties
        private Audio _selectedAudio;
        public Audio SelectedAudio
        {
            get { return _selectedAudio; }
            set
            {
                if (value == _selectedAudio) return;
                _selectedAudio = value;
                RaisePropertyChanged(nameof(SelectedAudio));
            }
        }

        private ObservableCollection<Playlist> _explorePlaylists;
        public ObservableCollection<Playlist> ExplorePlaylists
        {
            get { return _explorePlaylists; }
            set
            {
                if (value == _explorePlaylists) return;
                _explorePlaylists = value;
                RaisePropertyChanged(nameof(ExplorePlaylists));
            }
        }

        private string _user;
        public string User
        {
            get { return _user; }
            set
            {
                if (value == _user) return;
                _user = value;
                RaisePropertyChanged(nameof(User));
            }
        }

        private ObservableCollection<User> _users;
        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                if (value == _users) return;
                _users = value;
                RaisePropertyChanged(nameof(Users));
            }
        }

        private bool _isSpotifyCredsEntered;
        public bool IsSpotifyCredsEntered
        {
            get { return _isSpotifyCredsEntered; }
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
        public Playlist SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set
            {
                if (value == _selectedPlaylist) return;
                _selectedPlaylist = value;
                RaisePropertyChanged(nameof(SelectedPlaylist));

                if (_selectedPlaylist != null)
                { 
                    _selectedPlaylist.IsChecked = true;
                    GetPlaylistsAudios();
                }
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

            Users = new ObservableCollection<User>();

            _context = new DbContext();
            _audioRepository = new AudioRepository(_context);
            FetchDbData();
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

                if (string.IsNullOrEmpty(UserId) ||
                    string.IsNullOrEmpty(SecretId) ||
                    string.IsNullOrEmpty(RedirectUri) ||
                    string.IsNullOrEmpty(ServerUri)
                    )
                    return false;

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
                    await FetchSpotifyData();
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

                await FetchSpotifyData();
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
        private async void FetchDbData()
        {
            //await _audioRepository.InsertAsync(new Data.Model.Audio()
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    Artist = "Madeline Juno",
            //    Title = "Less than heartbreake",
            //    Href = "bing.com"
            //});

            var audios = await _audioRepository.GetAllAsync();

            await _audioRepository.RemoveAsync(audios.FirstOrDefault());

            var audio2 = await _audioRepository.GetAllAsync();
        }

        private async Task FetchSpotifyData()
        {
            IsSongsAreaBusy = IsPlaylistsAreaBusy = true;

            var playlists = (await _spotifyServices.GetAllPlaylists()).Select(x => new Playlist(x)).ToList();

            Playlists = new ObservableCollection<Playlist>(playlists);

            IsPlaylistsAreaBusy = false;

            likedSongs = (await _spotifyServices.GetSongs()).Select(x => new Audio(x.Track)).ToList();

            SavedTracks = new ObservableCollection<Audio>(likedSongs);

            var foreignUserPlaylists = await _spotifyServices.GetForeignUserPlaylists();

            IsSongsAreaBusy = false;
        }

        private async void GetPlaylistsAudios()
        {
            IsSongsAreaBusy = true;

            var audios = await _spotifyServices.GetPlaylistSongs(_selectedPlaylist.SimplePlaylist.Id);
            SavedTracks = new ObservableCollection<Audio>(audios.Select(x => new Audio(x.Track)));

            IsSongsAreaBusy = false;
        }
        #endregion

        #region commands
        private RelayCommand<Audio> _playSongCommand;
        public RelayCommand<Audio> PlaySongCommand => _playSongCommand ?? (_playSongCommand = new RelayCommand<Audio>(PlaySong));
        private async void PlaySong(Audio audio)
        {
            if (audio != null)
            {
                var playback = await _spotifyServices.PlayTrack(audio.Track.Uri);

                if (playback.HasError())
                    await ShowMessage("Playback Error", string.Format("Error code: {0}\n{1}\n{2}", playback.Error.Status, playback.Error.Message, "Make sure Spotify Client is opened and playback is working."));
            }
        }

        private bool checkExplorePlaylists = true;
        private RelayCommand _selectExplorePlaylistsCommand;
        public RelayCommand SelectExplorePlaylistsCommand => _selectExplorePlaylistsCommand ?? (_selectExplorePlaylistsCommand = new RelayCommand(SelectExplorePlaylists));
        private void SelectExplorePlaylists()
        {
            if (_explorePlaylists == null) return;

            foreach (var exploreplaylist in _explorePlaylists)
                exploreplaylist.IsChecked = checkExplorePlaylists;

            checkExplorePlaylists = !checkExplorePlaylists;
        }

        private bool checkAllPlaylists = true;
        private RelayCommand _selectAllMyPlaylistsCommand;
        public RelayCommand SelectAllMyPlaylistsCommand => _selectAllMyPlaylistsCommand ?? (_selectAllMyPlaylistsCommand = new RelayCommand(SelectAllMyPlaylists));
        private void SelectAllMyPlaylists()
        {
            if (_playlists == null) return;

            foreach (var playlist in _playlists)
                playlist.IsChecked = checkAllPlaylists;

            checkAllPlaylists = !checkAllPlaylists;
        }

        private RelayCommand _selectAllUsersCommand;
        public RelayCommand SelectAllUsersCommand => _selectAllUsersCommand ?? (_selectAllUsersCommand = new RelayCommand(SelectAllUsers));
        private void SelectAllUsers()
        {
            if (_users == null) return;

            foreach (var user in _users)
                user.IsChecked = !user.IsChecked;
        }

        private RelayCommand _getUsersPlaylistsCommand;
        public RelayCommand GetUsersPlaylistsCommand => _getUsersPlaylistsCommand ?? (_getUsersPlaylistsCommand = new RelayCommand(GetUsersPlaylists));
        private async void GetUsersPlaylists()
        {
            IsPlaylistsAreaBusy = true;

            var userPlaylists = (await _spotifyServices
                .GetForeignUserPlaylists(_users
                .Where(x => x.IsChecked)
                .Select(x => x.Username)
                .ToList()))
                .Select(x => new Playlist(x))
                .ToList();

            ExplorePlaylists = new ObservableCollection<Playlist>(userPlaylists);

            IsPlaylistsAreaBusy = false;
        }

        private RelayCommand _addUserCommand;
        public RelayCommand AddUserCommand => _addUserCommand ?? (_addUserCommand = new RelayCommand(AddUser));
        private void AddUser()
        {
            if (string.IsNullOrEmpty(_user)) return;

            _users.Add(new User(_user));

            User = string.Empty;
        }

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

            SelectedPlaylist = null;

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

                    if (_explorePlaylists != null)
                        selectedPlaylists.AddRange(_explorePlaylists.Where(x => x.IsChecked).Select(x => x.SimplePlaylist.Id));

                    var audiosOfPlaylists = new Dictionary<string, List<FullTrack>>();

                    foreach (var selectedPlaylist in selectedPlaylists)
                    {
                        var audiosOfPlaylist = (await _spotifyServices.GetPlaylistSongs(selectedPlaylist)).Select(x => x.Track).ToList();

                        if (audiosOfPlaylists.ContainsKey(selectedPlaylist))
                            continue;

                        audiosOfPlaylists.Add(selectedPlaylist, audiosOfPlaylist);
                    }

                    var trendAudios = new Dictionary<Audio, int>();

                    if (selectedAudios.Count == 0)
                        selectedAudios = new List<Audio>(audiosOfPlaylists.Values.SelectMany(x => x).Select(x => new Audio(x)));

                    var groupedAudios = selectedAudios.GroupBy(x => x.Track?.Id).Select(y =>
                    {
                        var audio = selectedAudios.First(z => z.Track?.Id == y.Key);

                        if (audio == null) return null;

                        audio.Hits = y.Count();

                        return audio;
                    })
                    .Where(x => x != null)
                    .Where(x => x.Hits >= _appearsAtLeastInX)
                    .OrderByDescending(x => x.Hits)
                    .ToList();

                    TrendTracks = new ObservableCollection<Audio>(groupedAudios);
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

                IsSpotifyCredsEntered = LoadSettings();

                if (IsSpotifyCredsEntered)
                    SpotifyProvider.InitProvider(_userId, _secretId, _redirectUri, _serverUri);
            }
            catch
            {
                IsSpotifyCredsEntered = false;
            }
        }
        #endregion
    }
}