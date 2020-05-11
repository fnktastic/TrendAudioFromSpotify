using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using MahApps.Metro.Controls.Dialogs;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using TrendAudioFromSpotify.Messaging;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Extensions;
using TrendAudioFromSpotify.UI.Messaging;
using TrendAudioFromSpotify.UI.Model;
using TrendAudioFromSpotify.UI.Service;
using TrendAudioFromSpotify.UI.Utility;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class SpotifyViewModel : ViewModelBase
    {
        #region fields
        private DispatcherTimer _refreshTokenTimer;

        private Token _token;

        private List<Audio> likedSongs;

        private readonly ISpotifyServices _spotifyServices;

        private readonly ISettingUtility _settingUtility;

        private readonly IDialogCoordinator _dialogCoordinator;

        private ProgressDialogController progressDialogController;

        private readonly IDataService _dataService;

        private readonly MonitoringViewModel _monitoringViewModel;

        private readonly GroupManagingViewModel _groupManagingViewModel;

        private readonly IMonitoringService _monitoringService;

        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region properties
        public CollectionViewSource FilteredAudioCollection { get; set; }
        public CollectionViewSource FilteredExplorePlaylistsCollection { get; set; }
        public CollectionViewSource FilteredMyPlaylistsCollection { get; set; }

        private bool _globalSearchEnabled;
        public bool GlobalSearchEnabled
        {
            get { return _globalSearchEnabled; }
            set
            {
                if (value == _globalSearchEnabled) return;
                _globalSearchEnabled = value;

                RaisePropertyChanged(nameof(GlobalSearchEnabled));
            }
        }

        private string _myPlaylistsSearchText;
        public string MyPlaylistsSearchText
        {
            get { return _myPlaylistsSearchText; }
            set
            {
                if (value == _myPlaylistsSearchText) return;
                _myPlaylistsSearchText = value;

                if (FilteredMyPlaylistsCollection.View != null)
                    FilteredMyPlaylistsCollection.View.Refresh();

                RaisePropertyChanged(nameof(MyPlaylistsSearchText));
            }
        }

        private string _explorePlaylistsSearchText;
        public string ExplorePlaylistsSearchText
        {
            get { return _explorePlaylistsSearchText; }
            set
            {
                if (value == _explorePlaylistsSearchText) return;
                _explorePlaylistsSearchText = value;

                if (FilteredExplorePlaylistsCollection.View != null)
                    FilteredExplorePlaylistsCollection.View.Refresh();

                if (string.IsNullOrWhiteSpace(_explorePlaylistsSearchText) == false)
                    CheckForUrl(_explorePlaylistsSearchText);

                RaisePropertyChanged(nameof(ExplorePlaylistsSearchText));
            }
        }

        private string _audiosSearchText;
        public string AudiosSearchText
        {
            get { return _audiosSearchText; }
            set
            {
                if (value == _audiosSearchText) return;
                _audiosSearchText = value;

                if (FilteredAudioCollection.View != null)
                    FilteredAudioCollection.View.Refresh();

                RaisePropertyChanged(nameof(AudiosSearchText));
            }
        }

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

        private PlaylistCollection _explorePlaylists;
        public PlaylistCollection ExplorePlaylists
        {
            get { return _explorePlaylists; }
            set
            {
                if (value == _explorePlaylists) return;
                _explorePlaylists = value;
                RaisePropertyChanged(nameof(ExplorePlaylists));

                if (_explorePlaylists != null)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        FilteredExplorePlaylistsCollection.Source = _explorePlaylists;
                    });
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

        private string _appearsTimesInX;
        public string AppearsTimesInX
        {
            get { return _appearsTimesInX; }
            set
            {
                if (value == _appearsTimesInX) return;
                _appearsTimesInX = value;
                RaisePropertyChanged(nameof(AppearsTimesInX));
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

        private PlaylistCollection _targetPlaylists;
        public PlaylistCollection TargetPlaylists
        {
            get { return _targetPlaylists; }
            set
            {
                if (value == _targetPlaylists) return;
                _targetPlaylists = value;
                RaisePropertyChanged(nameof(TargetPlaylists));
            }
        }

        private PlaylistCollection _playlists;
        public PlaylistCollection Playlists
        {
            get { return _playlists; }
            set
            {
                if (value == _playlists) return;
                _playlists = value;
                RaisePropertyChanged(nameof(Playlists));

                if (_playlists != null)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        FilteredMyPlaylistsCollection.Source = _playlists;
                    });
            }
        }

        private AudioCollection _savedTracks;
        public AudioCollection SavedTracks
        {
            get { return _savedTracks; }
            set
            {
                if (value == _savedTracks) return;
                _savedTracks = value;

                if (_savedTracks != null)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        FilteredAudioCollection.Source = _savedTracks;
                    });

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
                    GetPlaylistsAudios();
                }
            }
        }


        private Group _targetGroup;
        public Group TargetGroup
        {
            get { return _targetGroup; }
            set
            {
                if (value == _targetGroup) return;
                _targetGroup = value;
                RaisePropertyChanged(nameof(TargetGroup));
            }
        }

        private MonitoringItem _targetMonitoringItem;
        public MonitoringItem TargetMonitoringItem
        {
            get { return _targetMonitoringItem; }
            set
            {
                if (value == _targetMonitoringItem) return;
                _targetMonitoringItem = value;
                RaisePropertyChanged(nameof(TargetMonitoringItem));
            }
        }

        private TimeSpan _tokenExpiredIn;
        public TimeSpan TokenExpiredIn
        {
            get { return _tokenExpiredIn; }
            set
            {
                if (value == _tokenExpiredIn) return;
                _tokenExpiredIn = value;
                RaisePropertyChanged(nameof(TokenExpiredIn));
            }
        }
        #endregion

        public SpotifyViewModel(MonitoringViewModel monitoringViewModel, GroupManagingViewModel groupManagingViewModel, IDataService dataService, IMonitoringService monitoringService, ISpotifyServices spotifyServices, ISettingUtility settingUtility)
        {
            _refreshTokenTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _refreshTokenTimer.Tick += RefreshTokenTimer_Tick;

            _monitoringViewModel = monitoringViewModel;
            _groupManagingViewModel = groupManagingViewModel;
            _dataService = dataService;
            _monitoringService = monitoringService;
            _spotifyServices = spotifyServices;

            _dialogCoordinator = DialogCoordinator.Instance;
            _settingUtility = settingUtility;
            IsSpotifyCredsEntered = LoadSettings();

            Users = new ObservableCollection<User>();
            TargetPlaylists = new PlaylistCollection();
            TargetGroup = new Group();
            TargetMonitoringItem = new MonitoringItem();

            FilteredAudioCollection = new CollectionViewSource();
            FilteredAudioCollection.Filter += FilteredAudioCollection_Filter;

            FilteredExplorePlaylistsCollection = new CollectionViewSource();
            FilteredExplorePlaylistsCollection.Filter += FilteredExplorePlaylistsCollection_Filter;

            FilteredMyPlaylistsCollection = new CollectionViewSource();
            FilteredMyPlaylistsCollection.Filter += FilteredMyPlaylistsCollection_Filter;

            GlobalSearchEnabled = true;

            Messenger.Default.Register<RefreshSpotifyViewUI>(this, ResetUI);
            Messenger.Default.Register<AuthResponseMessage>(this, AuthResponse);
            Messenger.Default.Register<Token>(this, OnSpotifyTokenRecieved);
        }

        private async void RefreshTokenTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                TokenExpiredIn = TokenExpiredIn.Add(TimeSpan.FromSeconds(-1));

                if (TokenExpiredIn.TotalSeconds <= 600)
                {
                    var accessToken = _settingUtility.GetAccessToken();
                    var refreshToken = _settingUtility.GetRefreshToken();
                    await _spotifyServices.GetAccess(accessToken?.Value, refreshToken?.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnSpotifyTokenRecieved(Token token)
        {
            try
            {
                _token = token;

                if (string.IsNullOrEmpty(token.AccessToken) == false)
                    _settingUtility.SaveAccessToken(token.AccessToken);

                if (string.IsNullOrEmpty(token.RefreshToken) == false)
                    _settingUtility.SaveRefreshToken(token.RefreshToken);

                TokenExpiredIn = TimeSpan.FromSeconds(_token.ExpiresIn);

                _refreshTokenTimer.Start();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async void AuthResponse(AuthResponseMessage obj)
        {
            try
            {
                if (isStartup)
                {
                    IsConnectionEsatblished = true;

                    await HideConnectingMessage();

                    if (isStartup)
                    {
                        await ShowMessage("Notification", "Succesfully authorized in Spotify!");
                    }

                    var privateProfile = await _spotifyServices.GetPrivateProfile();

                    var publicProfile = await _spotifyServices.GetMyProfile();

                    Messenger.Default.Send<ConnectionEstablishedMessage>(new ConnectionEstablishedMessage(privateProfile, publicProfile));

                    await FetchSpotifyData();

                    isStartup = false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        #region dialogs
        private async Task ShowConnectingMessage()
        {
            try
            {
                progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Loading", "Connecting to Spotify...");
                progressDialogController.SetIndeterminate();

                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async Task ShowMessage(string header, string message)
        {
            try
            {
                await _dialogCoordinator.ShowMessageAsync(this, header, message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async Task HideConnectingMessage()
        {
            try
            {
                if (progressDialogController != null)
                    await progressDialogController.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Error in SpotifyViewModel.HideConnectingMessage", ex);
            }
        }
        #endregion

        #region private auth methods
        private bool LoadSettings()
        {
            try
            {
                _logger.Info("Load Settings...");

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
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }

        private async Task EstablishConnection()
        {
            try
            {
                _logger.Info("Connecting to Spotify...");

                var accessToken = _settingUtility.GetAccessToken();
                var refreshToken = _settingUtility.GetRefreshToken();

                await ShowConnectingMessage();

                string userId = _userId;
                string secretId = _secretId;
                string redirectUri = _redirectUri;
                string serverUri = _serverUri;

                _spotifyServices.Init(userId, secretId, redirectUri, serverUri);

                await _spotifyServices.GetAccess(accessToken?.Value, refreshToken?.Value);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private bool isStartup = true;
        #endregion

        #region private data metods
        private async void CheckForUrl(string s)
        {
            var uri = UrlValidator.Validate(s);

            if (uri != null)
            {
                string playlistId = uri.Segments.Last();

                var fullPlaylist = await _spotifyServices.GetPlaylistById(playlistId);

                var playlist = new Playlist(fullPlaylist.ToSimple())
                {
                    IsChecked = true
                };

                _targetPlaylists.Add(playlist);

                ExplorePlaylistsSearchText = string.Empty;
            }
        }

        private void ResetUI(object o)
        {
            try
            {
                TargetGroup = new Group();
                TargetPlaylists = new PlaylistCollection();

                if (ExplorePlaylists != null)
                    foreach (var playlist in ExplorePlaylists)
                        playlist.IsChecked = false;

                if (Playlists != null)
                    foreach (var playlist in Playlists)
                        playlist.IsChecked = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        void FilteredMyPlaylistsCollection_Filter(object sender, FilterEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_myPlaylistsSearchText))
                {
                    e.Accepted = true;
                    return;
                }

                if (e.Item != null)
                {
                    if (e.Item is Playlist playlist)
                    {
                        if (playlist.Name.ToUpper().Contains(_myPlaylistsSearchText.ToUpper()) ||
                            playlist.Owner.ToUpper().Contains(_myPlaylistsSearchText.ToUpper()))
                        {
                            e.Accepted = true;
                            return;
                        }
                    }
                }

                e.Accepted = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                e.Accepted = false;
            }
        }

        void FilteredExplorePlaylistsCollection_Filter(object sender, FilterEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_explorePlaylistsSearchText))
                {
                    e.Accepted = true;
                    return;
                }

                if (e.Item != null)
                {
                    if (e.Item is Playlist playlist)
                    {
                        if ((playlist.Name != null && playlist.Name.ToUpper().Contains(_explorePlaylistsSearchText.ToUpper())) ||
                            (playlist.Owner != null && playlist.Owner.ToUpper().Contains(_explorePlaylistsSearchText.ToUpper())))
                        {
                            e.Accepted = true;
                            return;
                        }
                    }
                }

                e.Accepted = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                e.Accepted = false;
            }
        }

        void FilteredAudioCollection_Filter(object sender, FilterEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_audiosSearchText))
                {
                    e.Accepted = true;
                    return;
                }

                if (e.Item != null)
                {
                    if (e.Item is Audio audio)
                    {
                        if (audio.Title.ToUpper().Contains(_audiosSearchText.ToUpper()) ||
                            audio.Artist.ToUpper().Contains(_audiosSearchText.ToUpper()))
                        {
                            e.Accepted = true;
                            return;
                        }
                    }
                }

                e.Accepted = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                e.Accepted = false;
            }
        }

        private void AddSpotifyUser()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_user)) return;

                _users.Add(new User(_user));

                User = string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async Task FetchSpotifyData()
        {
            try
            {
                IsSongsAreaBusy = IsPlaylistsAreaBusy = true;

                var playlists = (await _spotifyServices.GetAllPlaylists()).Select(x => new Playlist(x)).ToList();

                Playlists = new PlaylistCollection(playlists);

                IsPlaylistsAreaBusy = false;

                likedSongs = (await _spotifyServices.GetSongs()).Select(x => new Audio(x.Track)).ToList();

                SavedTracks = new AudioCollection(likedSongs);

                var foreignUserPlaylists = await _spotifyServices.GetForeignUserPlaylists();

                IsSongsAreaBusy = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async void GetPlaylistsAudios()
        {
            try
            {
                IsSongsAreaBusy = true;

                var audios = await _spotifyServices.GetPlaylistSongs(_selectedPlaylist.SpotifyId);
                SavedTracks = new AudioCollection(audios.Select(x => new Audio(x.Track)));

                IsSongsAreaBusy = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
        #endregion

        #region commands
        private RelayCommand _globalSearchCommand;
        public RelayCommand GlobalSearchCommand => _globalSearchCommand ?? (_globalSearchCommand = new RelayCommand(GlobalSearch));
        private async void GlobalSearch()
        {
            try
            {
                if (_globalSearchEnabled)
                {
                    try
                    {
                        IsPlaylistsAreaBusy = true;

                        var playlists = await _spotifyServices.GlobalPlaylistsSearch(_explorePlaylistsSearchText);

                        ExplorePlaylists = new PlaylistCollection(playlists.Select(x => new Playlist(x)));
                    }
                    finally
                    {
                        IsPlaylistsAreaBusy = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand<KeyEventArgs> _addSpotifyUsernameCommand;
        public RelayCommand<KeyEventArgs> AddSpotifyUsernameCommand => _addSpotifyUsernameCommand ?? (_addSpotifyUsernameCommand = new RelayCommand<KeyEventArgs>(AddSpotifyUsername));
        private void AddSpotifyUsername(KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter || e.Key == Key.Return)
                {
                    AddSpotifyUser();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand<Playlist> _playlistSelectedCommand;
        public RelayCommand<Playlist> PlaylistSelectedCommand => _playlistSelectedCommand ?? (_playlistSelectedCommand = new RelayCommand<Playlist>(PlaylistSelected));
        private void PlaylistSelected(Playlist playlist)
        {
            try
            {
                if (playlist.IsChecked)
                {
                    if (_targetPlaylists.Contains(playlist))
                        return;

                    _targetPlaylists.Add(playlist);
                }
                else
                    _targetPlaylists.Remove(playlist);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand<Audio> _saveAudioToDbCommand;
        public RelayCommand<Audio> SaveAudioToDbCommand => _saveAudioToDbCommand ?? (_saveAudioToDbCommand = new RelayCommand<Audio>(SaveAudioToDb));
        private async void SaveAudioToDb(Audio audio)
        {
            try
            {
                await _dataService.InsertAudioAsync(audio);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private bool checkExplorePlaylists = true;
        private RelayCommand _selectExplorePlaylistsCommand;
        public RelayCommand SelectExplorePlaylistsCommand => _selectExplorePlaylistsCommand ?? (_selectExplorePlaylistsCommand = new RelayCommand(SelectExplorePlaylists));
        private void SelectExplorePlaylists()
        {
            try
            {
                if (_explorePlaylists == null) return;

                foreach (var exploreplaylist in _explorePlaylists)
                {
                    exploreplaylist.IsChecked = checkExplorePlaylists;
                    PlaylistSelectedCommand.Execute(exploreplaylist);
                }

                checkExplorePlaylists = !checkExplorePlaylists;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private bool checkAllPlaylists = true;
        private RelayCommand _selectAllMyPlaylistsCommand;
        public RelayCommand SelectAllMyPlaylistsCommand => _selectAllMyPlaylistsCommand ?? (_selectAllMyPlaylistsCommand = new RelayCommand(SelectAllMyPlaylists));
        private void SelectAllMyPlaylists()
        {
            try
            {
                if (_playlists == null) return;

                foreach (var playlist in _playlists)
                {
                    playlist.IsChecked = checkAllPlaylists;
                    PlaylistSelectedCommand.Execute(playlist);
                }

                checkAllPlaylists = !checkAllPlaylists;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand _selectAllUsersCommand;
        public RelayCommand SelectAllUsersCommand => _selectAllUsersCommand ?? (_selectAllUsersCommand = new RelayCommand(SelectAllUsers));
        private void SelectAllUsers()
        {
            try
            {
                if (_users == null) return;

                foreach (var user in _users)
                    user.IsChecked = !user.IsChecked;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand _getUsersPlaylistsCommand;
        public RelayCommand GetUsersPlaylistsCommand => _getUsersPlaylistsCommand ?? (_getUsersPlaylistsCommand = new RelayCommand(GetUsersPlaylists));
        private async void GetUsersPlaylists()
        {
            try
            {
                IsPlaylistsAreaBusy = true;

                var userPlaylists = (await _spotifyServices
                    .GetForeignUserPlaylists(_users
                    .Where(x => x.IsChecked)
                    .Select(x => x.Username)
                    .ToList()))
                    .Select(x => new Playlist(x))
                    .ToList();

                ExplorePlaylists = new PlaylistCollection(userPlaylists);

                IsPlaylistsAreaBusy = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand _addUserCommand;
        public RelayCommand AddUserCommand => _addUserCommand ?? (_addUserCommand = new RelayCommand(AddUser));
        private void AddUser()
        {
            try
            {
                AddSpotifyUser();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand _connectToSpotifyCommand;
        public RelayCommand ConnectToSpotifyCommand => _connectToSpotifyCommand ?? (_connectToSpotifyCommand = new RelayCommand(ConnectToSpotify));
        private async void ConnectToSpotify()
        {
            try
            {
                await EstablishConnection();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand _likedSongsLoadCommand;
        public RelayCommand LikedSongsLoadCommand => _likedSongsLoadCommand ?? (_likedSongsLoadCommand = new RelayCommand(LikedSongsLoad));
        private void LikedSongsLoad()
        {
            try
            {
                IsSongsAreaBusy = true;

                SavedTracks = new AudioCollection(likedSongs);

                SelectedPlaylist = null;

                IsSongsAreaBusy = false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand _createProcessGroupCommand;
        public RelayCommand CreateProcessGroupCommand => _createProcessGroupCommand ?? (_createProcessGroupCommand = new RelayCommand(GetTrends));
        private async void GetTrends()
        {
            try
            {
                var monitoringItem = _monitoringService.Initiate(_targetGroup, _targetMonitoringItem, _targetPlaylists);

                if (monitoringItem.IsReady)
                {
                    _monitoringViewModel.MonitoringItems.Add(monitoringItem);

                    //monitoringItem.Group.MonitoringItem = monitoringItem;

                    _groupManagingViewModel.Groups.Add(monitoringItem.Group);

                    //move to service
                    await _dataService.InsertGroupAsync(monitoringItem.Group);
                    await _dataService.InsertPlaylistRangeAsync(monitoringItem.Group.Playlists);
                    await _dataService.InsertGroupPlaylistRangeAsync(monitoringItem.Group);

                    await _dataService.InsertMonitoringItemAsync(monitoringItem);
                    await _dataService.InsertPlaylistRangeAsync(monitoringItem.Group.Playlists);

                    ResetUI(null);

                    await _monitoringService.ProcessAsync(monitoringItem);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private RelayCommand _saveSpotifyCredentialsCommand;
        public RelayCommand SaveSpotifyCredentialsCommand => _saveSpotifyCredentialsCommand ?? (_saveSpotifyCredentialsCommand = new RelayCommand(SaveSpotifyCredentials));
        private async void SaveSpotifyCredentials()
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
                {
                    _logger.Info("Connecting to Spotify...");

                    await ShowConnectingMessage();

                    _spotifyServices.Init(userId, secretId, redirectUri, serverUri);

                    await _spotifyServices.GetAccess();
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                IsSpotifyCredsEntered = false;
            }
        }
        #endregion
    }
}