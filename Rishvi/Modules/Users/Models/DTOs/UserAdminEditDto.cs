using System;

namespace Rishvi.Modules.Users.Models.DTOs
{
    public class UserAdminEditDto
    {
        public Guid UserId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string EmailAddress { get; set; }
        public string Username { get; set; }
        public string Company { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? LinnworksId { get; set; }
        public Guid? LinnworksApplicationToken { get; set; }
        public Guid? LinnworksUserToken { get; set; }
        public string LinnworksServerUrl { get; set; }
        public string KlaviyoPrivateKey { get; set; }
        public string KlaviyoPublicKey { get; set; }
        public string KlaviyoEmail { get; set; }
    }
}
