using System;

namespace LinnworksAPI
{
    public class CheckinUserResponse
    {
        public DateTime? LastRun { get; set; }

        public Boolean AllocationStarted { get; set; }

        public Boolean AllocationRunning { get; set; }
    }
}