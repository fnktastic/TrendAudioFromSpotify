using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TrendAudioFromSpotify.UI.Model;
using TrendAudioFromSpotify.UI.Utility;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class SchedulingViewModelDialog : ViewModelBase
    {
        public Schedule Schedule { get; set; }

        private readonly ICommand _closeCommand;
        public bool IsCanceled;

        public SchedulingViewModelDialog(Action<SchedulingViewModelDialog> closeHandler)
        {
            _closeCommand = new SimpleCommand
            {
                ExecuteDelegate = o => closeHandler(this)
            };

            Schedule = new Schedule();
        }

        public RelayCommand IsCanceledCommand => new RelayCommand(this.Canceled);
        public ICommand CloseCommand
        {
            get { return _closeCommand; }
        }

        public void Canceled()
        {
            IsCanceled = true;
            if (CloseCommand.CanExecute(null))
            {
                CloseCommand.Execute(null);
            }
        }
    }
}
