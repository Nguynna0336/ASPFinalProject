using System.ComponentModel.DataAnnotations;

namespace ASPFinalProject.DTOs.User
{
    public class ChangePasswordDTO
    {
        [Required]
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        [Compare("NewPassword", ErrorMessage = "Password and confirm password does not match")]
        public string ConfirmPassword { get; set; }
    }
}
