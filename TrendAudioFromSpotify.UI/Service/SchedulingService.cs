using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Enum;
using TrendAudioFromSpotify.UI.Job;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Service
{
    public interface ISchedulingService
    {
        Task ScheduleMonitoringItem(MonitoringItem monitoringItem);
        Task<Dictionary<Guid, DateTimeOffset>> GetActiveSchedulings();
    }

    public class SchedulingService : ISchedulingService
    {
        private readonly IScheduler _scheduler;

        public async Task<Dictionary<Guid, DateTimeOffset>> GetActiveSchedulings()
        {
            var sheduleDetails = new Dictionary<Guid, DateTimeOffset>();

            var jobKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());

            foreach (JobKey jobKey in jobKeys)
            {
                var detail = await _scheduler.GetJobDetail(jobKey);
                var triggers = (await _scheduler.GetTriggersOfJob(jobKey)).Select(x => x);

                foreach (var trigger in triggers)
                {
                    DateTimeOffset? nextFireTime = trigger.GetNextFireTimeUtc();
                    if (nextFireTime.HasValue)
                    {
                        Console.WriteLine(nextFireTime.Value.LocalDateTime.ToString());

                        Guid monitoringItem = Guid.Parse(detail.JobDataMap.FirstOrDefault(x => x.Key == "monitoringItemId").Value.ToString());

                        sheduleDetails.Add(monitoringItem, nextFireTime.Value);
                    }
                }
            }

            return sheduleDetails;
        }

        public SchedulingService()
        {
            _scheduler = StdSchedulerFactory.GetDefaultScheduler().GetAwaiter().GetResult();
        }

        public async Task ScheduleMonitoringItem(MonitoringItem monitoringItem)
        {
            if (monitoringItem.Schedule.RepeatOn)
            {
                if (_scheduler.IsStarted == false)
                    await _scheduler.Start();

                var jobKey = new JobKey(monitoringItem.Id.ToString(), "monitorItemGroup");
                if (await _scheduler.CheckExists(jobKey) == false)
                {
                    IJobDetail job = JobBuilder.Create<MonitorItemJob>()
                    .WithIdentity(monitoringItem.Id.ToString(), "monitorItemGroup")
                    .UsingJobData("monitoringItemId", monitoringItem.Id.ToString())
                    .Build();

                    ITrigger trigger = null;

                    if (monitoringItem.Schedule.RepeatMode == RepeatModeEnum.SpecificDay)
                    {
                        trigger = TriggerBuilder.Create()
                           .WithIdentity(monitoringItem.Id.ToString(), "monitorItemGroup")
                           .StartAt(monitoringItem.Schedule.StartDateTime.Value)
                           .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(monitoringItem.Schedule.DayOfWeek.Value, monitoringItem.Schedule.StartDateTime.Value.Hour, monitoringItem.Schedule.StartDateTime.Value.Minute))
                           .Build();
                    }
                    else
                    {
                        int interval = ConvertToSeconds(monitoringItem.Schedule);

                        trigger = TriggerBuilder.Create()
                            .WithIdentity(monitoringItem.Id.ToString(), "monitorItemGroup")
                            .StartAt(monitoringItem.Schedule.StartDateTime.Value)
                            .WithSimpleSchedule(x => x
                                .WithIntervalInHours(interval)
                                .RepeatForever())
                            .Build();
                    }

                    await _scheduler.ScheduleJob(job, trigger);
                }
            }
        }

        private int ConvertToSeconds(Schedule schedule)
        {
            int interval = schedule.RepeatInterval;

            double hours = 0;

            switch (schedule.RepeatMode)
            {
                case RepeatModeEnum.Hourly:
                    hours = TimeSpan.FromHours(interval).TotalHours;
                    break;
                case RepeatModeEnum.Daily:
                    hours = TimeSpan.FromDays(interval).TotalHours;
                    break;
                case RepeatModeEnum.Weekly:
                    hours = TimeSpan.FromDays(interval * 7).TotalHours;
                    break;
                case RepeatModeEnum.Monthly:
                    hours = TimeSpan.FromDays(interval * 30).TotalHours;
                    break;
            }

            return int.Parse(hours.ToString());
        }
    }
}
