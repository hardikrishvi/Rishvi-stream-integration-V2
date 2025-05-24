namespace Rishvi.Modules.AdminUsers.Admin.Models.DTOs
{
    public class AdminResetPasswordDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}