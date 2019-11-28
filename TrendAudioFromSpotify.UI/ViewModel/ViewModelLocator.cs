using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();

            SimpleIoc.Default.Register<MonitoringViewModel>();
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }
        
        public MonitoringViewModel Monitoring
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MonitoringViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}