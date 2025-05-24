using System;
using System.Collections.Generic;

namespace Rishvi.Modules.AdminUsers.Admin.Models.DTOs
{
    public class AdminUserEditDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public List<Guid> Roles { get; set; }
    }
}
