using Rishvi.Core.Data;

namespace Rishvi.Models
{
    public class Run : IModificationHistory
    {
        public int id { get; set; }
        public string loadId { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
