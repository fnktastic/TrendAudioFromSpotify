using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using TrendAudioFromSpotify.Data.Repository;
using TrendAudioFromSpotify.UI.Utility;
using DbContext = TrendAudioFromSpotify.Data.DataAccess.Context;

namespace TrendAudioFromSpotify.UI.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<SpotifyViewModel>();

            SimpleIoc.Default.Register<MonitoringViewModel>();

            SimpleIoc.Default.Register<DbContext>();

            SimpleIoc.Default.Register<SerialQueue>();

            SimpleIoc.Default.Register<IAudioRepository, AudioRepository>();
        }

        public SpotifyViewModel Spotify
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SpotifyViewModel>();
            }
        }
        
        public MonitoringViewModel Monitoring
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MonitoringViewModel>();
            }
        }

        public DbContext DbContext
        {
            get
            {
                return ServiceLocator.Current.GetInstance<DbContext>();
            }
        }

        public SerialQueue SerialQueue
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SerialQueue>();
            }
        }

        public IAudioRepository AudioRepository
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AudioRepository>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}