using System.ComponentModel.DataAnnotations;

namespace Rishvi.Models
{
    public class WebhookOrder
    {
        [Key]
        public int id { get; set; }
        public int sequence { get; set; }
        public string order { get; set; }
    }
}
