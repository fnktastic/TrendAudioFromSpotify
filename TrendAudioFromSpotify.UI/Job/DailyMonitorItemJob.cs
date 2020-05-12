using CommonServiceLocator;
using GalaSoft.MvvmLight.Messaging;
using Quartz;
using System.Linq;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Service.Spotify;
using TrendAudioFromSpotify.UI.Messaging;

namespace TrendAudioFromSpotify.UI.Job
{
    public class DailyMonitorItemJob : IJob
    {
        private readonly ISpotifyServices _spotifyServices;

        public DailyMonitorItemJob()
        {
            _spotifyServices = ServiceLocator.Current.GetInstance<ISpotifyServices>();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string monitoringItemId = context.JobDetail.JobDataMap.FirstOrDefault(x => x.Key == "monitoringItemId").Value.ToString();

            Messenger.Default.Send<StartDailyMonitoringMessage>(new StartDailyMonitoringMessage(monitoringItemId));
        }
    }
}
