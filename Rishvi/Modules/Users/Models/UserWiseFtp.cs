using System;
using System.Collections.Generic;

namespace Rishvi.Modules.Users.Models
{
    public class UserWiseFtp
    {
        public Guid UserWiseFtpId { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public Boolean IsSSL { get; set; }
    }
}
