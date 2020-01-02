using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Job;
using TrendAudioFromSpotify.UI.Model;
using TrendAudioFromSpotify.UI.Enum;
using Quartz.Impl.Calendar;

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

            var jobKey = new JobKey(monitoringItem.Id.ToString(), "monitorItemGroup");
            if (await scheduler.CheckExists(jobKey) == false)
            {
                IJobDetail job = JobBuilder.Create<MonitorItemJob>()
                .WithIdentity(monitoringItem.Id.ToString(), "monitorItemGroup")
                .UsingJobData("monitoringItemId", monitoringItem.Id.ToString())
                .Build();

                ITrigger trigger = null;

                if (monitoringItem.Schedule.RepeatMode == RepeatModeEnum.SpecificDay)
                {
                    string calendarName = string.Format("includedDays_{0}", monitoringItem.Id);
                    WeeklyCalendar cal = new WeeklyCalendar();
                    cal.SetDayExcluded(monitoringItem.Schedule.DayOfWeek.Value, false);
                    await scheduler.AddCalendar(calendarName, cal, false, false);


                    trigger = TriggerBuilder.Create()
                       .WithIdentity(monitoringItem.Id.ToString(), "monitorItemGroup")
                       .StartAt(monitoringItem.Schedule.StartDateTime.Value)
                       .ModifiedByCalendar(calendarName)
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

                await scheduler.ScheduleJob(job, trigger);
            }
        }

        private int ConvertToSeconds(Schedule schedule)
        {
            int interval = schedule.RepeatInterval;

            double hours = 0;

            switch(schedule.RepeatMode)
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
