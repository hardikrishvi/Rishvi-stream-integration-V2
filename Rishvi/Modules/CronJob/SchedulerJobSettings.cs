namespace Rishvi.Modules.CronJob
{
    public class SchedulerJobSettings
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string CronExpression { get; set; }
        public bool IsActive { get; set; }
        public IList<string> ToEmail { get; set; }
    }

    public enum SchedulerJob
    {
        MyJob,
        OnDemandJob,
        TempJob
    }
}
