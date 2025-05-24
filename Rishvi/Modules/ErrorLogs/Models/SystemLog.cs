using Rishvi.Modules.Core.Data;
using System;
namespace Rishvi.Modules.ErrorLogs.Models
{
    public class SystemLog : IModificationHistory
    {
        public Guid SystemLogID { get; set; }
        public Guid? UserId { get; set; }
        public string ModuleName { get; set; }
        public string RequestHeader { get; set; }
        public string RequestJson { get; set; }
        public string ResponseJson { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
