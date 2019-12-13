using AutoMapper;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using TrendAudioFromSpotify.Data.Model;
using TrendAudioFromSpotify.Data.Repository;
using TrendAudioFromSpotify.UI.Model;
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

            SimpleIoc.Default.Register<GroupManagingViewModel>();

            SimpleIoc.Default.Register<DbContext>();

            SimpleIoc.Default.Register<SerialQueue>();

            SimpleIoc.Default.Register<IAudioRepository, AudioRepository>();

            SimpleIoc.Default.Register<IPlaylistRepository, PlaylistRepository>();

            SimpleIoc.Default.Register<IPlaylistAudioRepository, PlaylistAudioRepository>();

            SimpleIoc.Default.Register<IGroupRepository, GroupRepository>();

            SimpleIoc.Default.Register<IGroupPlaylistRepository, GroupPlaylistRepository>();

            SimpleIoc.Default.Register<IConfigurationProvider, MyConfig>();

            SimpleIoc.Default.Register<IMapper, MyMapper> ();
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

        public GroupManagingViewModel GroupManaging
        {
            get
            {
                return ServiceLocator.Current.GetInstance<GroupManagingViewModel>();
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

        public IPlaylistRepository PlaylistRepository
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PlaylistRepository>();
            }
        }

        public IPlaylistAudioRepository PlaylistAudioRepository
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PlaylistAudioRepository>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}