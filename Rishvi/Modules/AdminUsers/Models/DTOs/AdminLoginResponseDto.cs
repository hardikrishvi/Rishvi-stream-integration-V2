using System;
using System.Collections.Generic;

namespace Rishvi.Modules.AdminUsers.Admin.Models.DTOs
{
    public class AdminLoginResponseDto
    {
        public string AuthorizationKey { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<LoginUserPermissions> Permissions { get; set; }
    }

    public class LoginUserPermissions
    {
        public Guid PermissionId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }
}