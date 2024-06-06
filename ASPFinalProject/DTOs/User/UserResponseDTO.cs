using System.ComponentModel.DataAnnotations;

namespace ASPFinalProject.DTOs.User
{
    public class UserResponseDTO
    {
        public int Id {  get; set; }
        public string Username { get; set; } = null!;

        public string Fullname { get; set; } = null!;

        [EmailAddress]
        public string? Email { get; set; }
        public string RoleName { get; set; } = null!;
    }
}
