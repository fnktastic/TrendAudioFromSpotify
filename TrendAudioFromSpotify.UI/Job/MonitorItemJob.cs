﻿using CommonServiceLocator;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Service.Spotify;

namespace TrendAudioFromSpotify.UI.Job
{
    public class MonitorItemJob : IJob
    {
        private readonly ISpotifyServices _spotifyServices;

        public MonitorItemJob()
        {
            _spotifyServices = ServiceLocator.Current.GetInstance<ISpotifyServices>();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string monitoringItemId = context.JobDetail.JobDataMap.FirstOrDefault(x => x.Key == "monitoringItemId").Value.ToString();

            Console.WriteLine("QUARTZ MonitorItemJob {0}", monitoringItemId);
        }
    }
}
