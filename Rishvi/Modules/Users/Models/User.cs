
using Rishvi.Modules.LabelTemplateSettings.Models;
using System;
using System.Collections.Generic;

namespace Rishvi.Modules.Users.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string EmailAddress { get; set; }
        public string Username { get; set; }
        public string Company { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public Guid? LinnworksId { get; set; }
        public Guid? LinnworksApplicationToken { get; set; }
        public Guid? LinnworksUserToken { get; set; }
        public string LinnworksServerUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        //public string Password { get; set; }
        //public string Salt { get; set; }
        //public string ForgotPasswordResetCode { get; set; }
        //public string Address1 { get; set; }
        //public string Address2 { get; set; }
        //public string City { get; set; }
        //public string Country { get; set; }
        //public string PhoneNumber { get; set; }
        //public string StripCustomerId { get; set; }
        public List<UserWiseFtp> UserWiseFtp { get; set; }
        public List<LabelTemplateSetting> LabelTemplateSettings { get; set; }
    }
}
