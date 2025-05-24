using System;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Rishvi.Modules.CronJob
{
    public class HangfireJobExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
    {
        private readonly int _expTime;
        public HangfireJobExpirationTimeAttribute(int expTime)
        {
            _expTime = expTime == 0 ? 90 : expTime;
        }
        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {

            context.JobExpirationTimeout = TimeSpan.FromDays(_expTime);
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(_expTime);
        }
    }
}
