using System;

namespace Rishvi.Modules.Users.Models.DTOs
{
    public class UserLinnworksDto
    {
        public Guid? LinnworksId { get; set; }
        public Guid? LinnworksApplicationToken { get; set; }
        public Guid? LinnworksUserToken { get; set; }
        public string LinnworksServerUrl { get; set; }
    }
}
