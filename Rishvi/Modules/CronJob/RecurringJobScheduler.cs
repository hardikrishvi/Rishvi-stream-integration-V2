using Hangfire;
using Rishvi.Modules.Core.Extensions;

namespace Rishvi.Modules.CronJob
{
    public class RecurringJobScheduler
    {
        public static void ScheduleRecurringJobs(List<SchedulerJobSettings> jobSettings)
        {
            if (jobSettings != null)
            {
                var myJob = jobSettings.GetJob(SchedulerJob.MyJob.ToString());
                RecurringJob.RemoveIfExists(myJob.Name);
                if (myJob.IsActive)
                {
                    //RecurringJob.AddOrUpdate<MyJobNotification>(myJob.Name, job => job.SendWithPrepareAsync(true), myJob.CronExpression);
                }

                var onDemandJob = jobSettings.GetJob(SchedulerJob.OnDemandJob.ToString());
                RecurringJob.RemoveIfExists(onDemandJob.Name);
                if (onDemandJob.IsActive)
                {
                    RecurringJob.AddOrUpdate(onDemandJob.Name, () => Console.Write("OnDemand Job!"), onDemandJob.CronExpression);
                }

                var tempJob = jobSettings.GetJob(SchedulerJob.TempJob.ToString());
                RecurringJob.RemoveIfExists(tempJob.Name);
                if (tempJob.IsActive)
                {
                    RecurringJob.AddOrUpdate(tempJob.Name, () => Console.Write("Temp Job!"), tempJob.CronExpression);
                    RecurringJob.Trigger(tempJob.Name);
                    RecurringJob.RemoveIfExists(tempJob.Name);
                }
            }
        }
    }
}
