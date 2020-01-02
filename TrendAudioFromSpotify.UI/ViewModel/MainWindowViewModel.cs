using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Enum;
using TrendAudioFromSpotify.UI.Messaging;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region fields
        private readonly ISpotifyServices _spotifyServices;
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDialogCoordinator _dialogCoordinator;
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
        #endregion

        #region constructor
        public MainWindowViewModel(ISpotifyServices spotifyServices)
        {
            _dialogCoordinator = DialogCoordinator.Instance;
            _spotifyServices = spotifyServices;
            Messenger.Default.Register<TabsEnum>(this, ReceiveSelectGroupMessage);
            Messenger.Default.Register<PlayAudioMessage>(this, RecievePlayAudioMessage);
        }
        #endregion

        #region messages
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
    }
}
