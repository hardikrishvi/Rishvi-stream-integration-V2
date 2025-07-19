using Rishvi.Modules.Core.Data;

namespace Rishvi.Models
{
    public class PostalServices : IModificationHistory
    {
        public int Id { get; set; }
        public string AuthorizationToken { get; set; }
        public string PostalServiceId { get; set; }
        public string PostalServiceName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
