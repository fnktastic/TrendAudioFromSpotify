using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Service
{
    public interface IGroupService
    {
        Task MonitorGroupAsync(Group group);
    }

    public class GroupService : IGroupService
    {
        private readonly IMonitoringService _monitoringService;

        public GroupService(IMonitoringService monitoringService)
        {
            _monitoringService = monitoringService;
        }

        public async Task MonitorGroupAsync(Group group)
        {

        }

    }
}
