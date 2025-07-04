using System.ComponentModel.DataAnnotations;

namespace Rishvi.Models
{

    public class ClientAuth
    {
        [Key]
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string AccessToken { get; set; }
        public DateTime ExpireTime { get; set; }
        public string scope { get; set; }
        public string TokenType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
