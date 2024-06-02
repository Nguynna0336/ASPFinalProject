using System.ComponentModel.DataAnnotations;

namespace ASPFinalProject.DTOs.User
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Username cannot be blank")]

        public string Username { get; set; } = null!;
        [Required(ErrorMessage = "Password cannot be blank")]
        public string Password { get; set; } = null!;
        [Required(ErrorMessage = "Confirm Password cannot be blank")]
        [Compare("Password", ErrorMessage = "Password and confirm password does not match")]
        public string ConfirmPassword { get; set; } = null;
        [Required(ErrorMessage = "Full name cannot be blank")]
        public string Fullname { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Invalid email")]
        public string? Email { get; set; }
    }
}
