using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Model;
using TrendAudioFromSpotify.UI.Service;
using TrendAudioFromSpotify.UI.Utility;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class MonitoringViewModel : ViewModelBase
    {
        #region fields
        public ISpotifyServices SpotifyServices = null;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IDataService _dataService;
        #endregion

        #region properties
        private ObservableCollection<Group> _groups;
        public ObservableCollection<Group> Groups
        {
            get { return _groups; }
            set
            {
                if (value == _groups) return;
                _groups = value;
                RaisePropertyChanged(nameof(Groups));
            }
        }

        private Group _selectedGroup;
        public Group SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                if (value == _selectedGroup) return;
                _selectedGroup = value;
                RaisePropertyChanged(nameof(SelectedGroup));
            }
        }
        #endregion

        public MonitoringViewModel(IDataService dataService)
        {
            _dialogCoordinator = DialogCoordinator.Instance;
            _dataService = dataService;

            Groups = new ObservableCollection<Group>();
        }

        #region dialogs
        private async Task ShowMessage(string header, string message)
        {
            await _dialogCoordinator.ShowMessageAsync(this, header, message);
        }
        #endregion

        #region commands
        private RelayCommand<Audio> _playSongCommand;
        public RelayCommand<Audio> PlaySongCommand => _playSongCommand ?? (_playSongCommand = new RelayCommand<Audio>(PlaySong));
        private async void PlaySong(Audio audio)
        {
            if (audio != null)
            {
                var playback = await SpotifyServices.PlayTrack(audio.Uri);

                if (playback.HasError())
                    await ShowMessage("Playback Error", string.Format("Error code: {0}\n{1}\n{2}", playback.Error.Status, playback.Error.Message, "Make sure Spotify Client is opened and playback is working."));
            }
        }
        #endregion
    }
}
