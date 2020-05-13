using GalaSoft.MvvmLight.Messaging;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Collections;
using TrendAudioFromSpotify.UI.Messaging;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Service
{
    public interface IGroupService
    {
        Task MonitorGroupAsync(ISpotifyServices spotifyServices, Group group);
    }

    public class GroupService : IGroupService
    {
        private readonly IMonitoringService _monitoringService;
        private readonly IDataService _dataService;

        public GroupService(IMonitoringService monitoringService, IDataService dataService)
        {
            _monitoringService = monitoringService;
            _dataService = dataService;
        }

        public async Task MonitorGroupAsync(ISpotifyServices spotifyServices, Group group)
        {
            var monitoringItem = _monitoringService.Initiate(group, group.GroupSourceMonitoringItem, group.Playlists);

            if (monitoringItem != null && monitoringItem.IsReady)
            {
                group.GroupSourceMonitoringItem = new MonitoringItem();

                Messenger.Default.Send<AddMonitoringItemMessage>(new AddMonitoringItemMessage(monitoringItem));

                //move to service
                await _dataService.InsertMonitoringItemAsync(monitoringItem);
                await _dataService.InsertPlaylistRangeAsync(monitoringItem.Group.Playlists);

                await _monitoringService.ProcessAsync(monitoringItem);
            }
        }

    }
}
