using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Job;

namespace TrendAudioFromSpotify.UI.Service
{
    public interface ISchedulingService
    {
        Task ScheduleMonitoringItem();
    }

    public class SchedulingService : ISchedulingService
    {
        public async Task ScheduleMonitoringItem()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();

            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<MonitorItemJob>()
                .WithIdentity("monitorItem", "monitorItemGroup")
                .UsingJobData("monitoringItemId", "xxx")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("monitorItemTrigger", "monitorItemGroup")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(10)
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}
