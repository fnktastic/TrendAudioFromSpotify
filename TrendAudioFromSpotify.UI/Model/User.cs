using GalaSoft.MvvmLight;

namespace TrendAudioFromSpotify.UI.Model
{
    public class User : ViewModelBase
    {
        public string Username { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;
                RaisePropertyChanged(nameof(IsChecked));
            }
        }

        public User(string username)
        {
            Username = username;
        }
    }
}
