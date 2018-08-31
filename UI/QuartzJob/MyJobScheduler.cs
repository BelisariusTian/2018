using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace UI.QuartzJob
{
	/// <summary>
	/// 定时任务触发器
	/// </summary>
	public class MyJobScheduler
	{
		private static readonly string cronStr = ConfigurationManager.AppSettings["cronStr"];
		public static void Run() {
			
			IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
			
			scheduler.Start();
			IJobDetail job = JobBuilder.Create<CommissionJob>().Build();
			ICronTrigger trigger = (ICronTrigger)TriggerBuilder.Create()
													.WithIdentity("trigger1", "group1")
													.WithCronSchedule("0 0 0 1/1 * ?")//  //1 0 0 1/1 * ?   //1 0 10 1/1 * ? 
													.Build();
			scheduler.ScheduleJob(job, trigger);
			
		}
	}
}