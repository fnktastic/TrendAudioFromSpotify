using CommonServiceLocator;
using GalaSoft.MvvmLight.Messaging;
using Quartz;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Messaging;

namespace TrendAudioFromSpotify.UI.Job
{
    public class CheckExportedPlaylists : IJob
    {
        private readonly ISpotifyServices _spotifyServices;

        public CheckExportedPlaylists()
        {
            _spotifyServices = ServiceLocator.Current.GetInstance<ISpotifyServices>();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Messenger.Default.Send<CheckExportedPlaylistsMessage>(new CheckExportedPlaylistsMessage());
        }
    }
}
