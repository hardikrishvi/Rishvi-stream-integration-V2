using System;

namespace Rishvi.Modules.AdminUsers.Admin.Models.DTOs
{
    public class AdminChangePasswordDto
    {
        public Guid Id { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}