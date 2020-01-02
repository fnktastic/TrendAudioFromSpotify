using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Job;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Service
{
    public interface ISchedulingService
    {
        Task ScheduleMonitoringItem(MonitoringItem monitoringItem);
    }

    public class SchedulingService : ISchedulingService
    {
        public async Task ScheduleMonitoringItem(MonitoringItem monitoringItem)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();

            if (scheduler.IsStarted == false)
                await scheduler.Start();

            IJobDetail job = JobBuilder.Create<MonitorItemJob>()
                .WithIdentity(monitoringItem.Id.ToString(), "monitorItemGroup")
                .UsingJobData("monitoringItemId", monitoringItem.Id.ToString())
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(monitoringItem.Id.ToString(), "monitorItemGroup")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(10)
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}
