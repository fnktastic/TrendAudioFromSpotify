using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using MahApps.Metro.Controls.Dialogs;
using SpotifyAPI.Web.Models;
using System;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Enum;
using TrendAudioFromSpotify.UI.Messaging;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region fields
        private readonly ISpotifyServices _spotifyServices;
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDialogCoordinator _dialogCoordinator;
        private PrivateProfile privateProfile;
        private PublicProfile publicProfile;
        #endregion

        #region properties
        private TabsEnum _selecteedTab;
        public TabsEnum SelectedTab
        {
            get { return _selecteedTab; }
            set
            {
                if (value == _selecteedTab) return;
                _selecteedTab = value;
                RaisePropertyChanged(nameof(SelectedTab));
            }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                if (_username == value) return;
                _username = value;
                RaisePropertyChanged(nameof(Username));
            }
        }
        #endregion

        #region constructor
        public MainWindowViewModel(ISpotifyServices spotifyServices)
        {
            Username = "Not logged in. Please, do login.";

            _dialogCoordinator = DialogCoordinator.Instance;
            _spotifyServices = spotifyServices;
            Messenger.Default.Register<TabsEnum>(this, ReceiveSelectGroupMessage);
            Messenger.Default.Register<PlayAudioMessage>(this, RecievePlayAudioMessage);
            Messenger.Default.Register<ConnectionEstablishedMessage>(this, RecieveConnectionEstablishedMessage);
        }
        #endregion

        #region messages
        private void RecieveConnectionEstablishedMessage(ConnectionEstablishedMessage obj)
        {
            if (obj.PublicProfile != null && obj.PrivateProfile != null)
            {
                privateProfile = obj.PrivateProfile;
                publicProfile = obj.PublicProfile;

                Username = string.Format("{0}({1}) | {2}", privateProfile.DisplayName, privateProfile.Product, privateProfile.Country);
            }
        }

        private async void RecievePlayAudioMessage(PlayAudioMessage playAudioMessage)
        {
            try
            {
                var audio = playAudioMessage.Audio;

                if (audio != null)
                {
                    var playback = await _spotifyServices.PlayTrack(audio.Uri);

                    if (playback.HasError())
                        await ShowMessage("Playback Error", string.Format("Error code: {0}\n{1}\n{2}", playback.Error.Status, playback.Error.Message, "Make sure Spotify Client is opened and playback is working."));
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in MonitoringViewModel.PlaySong", ex);
                await ShowMessage("Playback Error", string.Format("Error code: {0}\n{1}", ex.Message, "Make sure Spotify Client is opened and playback is working."));
            }
        }

        private void ReceiveSelectGroupMessage(TabsEnum tab)
        {
            SelectedTab = tab;
        }
        #endregion

        #region dialogs
        private async Task ShowMessage(string header, string message)
        {
            try
            {
                await _dialogCoordinator.ShowMessageAsync(this, header, message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error in MonitoringViewModel.ShowMessage", ex);
            }
        }
        #endregion

        #region commands
        private RelayCommand _openUserInBrowserCommand;
        public RelayCommand OpenUserInBrowserCommand => _openUserInBrowserCommand ?? (_openUserInBrowserCommand = new RelayCommand(OpenUserInBrowser));
        private void OpenUserInBrowser()
        {
            try
            {
                if (publicProfile != null)
                {
                    string url = publicProfile.ExternalUrls["spotify"];

                    System.Diagnostics.Process.Start(url);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
        #endregion
    }
}
