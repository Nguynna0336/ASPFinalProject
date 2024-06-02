using System.ComponentModel.DataAnnotations;

namespace ASPFinalProject.DTOs.User
{
    public class LoginDTO
    {
        [Required]
        public string userName { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public bool rememberMe { get; set; }
    }
}
