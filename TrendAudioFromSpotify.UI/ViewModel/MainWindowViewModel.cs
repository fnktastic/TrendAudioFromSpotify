using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Enum;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
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

        public MainWindowViewModel()
        {
            Messenger.Default.Register<TabsEnum>(this, ReceiveSelectGroupMessage);
        }

        #region messages
        private void ReceiveSelectGroupMessage(TabsEnum tab)
        {
            SelectedTab = tab;
        }
        #endregion
    }
}
