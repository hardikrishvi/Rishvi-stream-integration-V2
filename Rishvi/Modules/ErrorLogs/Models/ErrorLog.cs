using Rishvi.Modules.Core.Data;
namespace Rishvi.Modules.ErrorLogs.Models
{
    public class ErrorLog : IModificationHistory
    {
        public Guid ErrorLogID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
