using Rishvi.Modules.CronJob;

namespace Rishvi.Modules.Core.Extensions
{
    public static class CronJobExtensions
    {
        public static SchedulerJobSettings GetJob(this List<SchedulerJobSettings> settings, string jobName)
        {
            return settings.FirstOrDefault(a => a.Name == jobName);
        }
    }
}
