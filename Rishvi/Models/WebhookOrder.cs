using System.ComponentModel.DataAnnotations;
using Rishvi.Core.Data;

namespace Rishvi.Models
{
    public class WebhookOrder : IModificationHistory
    {
        [Key]
        public int id { get; set; }
        public int sequence { get; set; }
        public string order { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
