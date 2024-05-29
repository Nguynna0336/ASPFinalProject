using MySqlX.XDevAPI.Common;
using static System.Net.Mime.MediaTypeNames;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPFinalProject.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [StringLength(20, ErrorMessage ="username max length is 20")]
        public string Username { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = null!;

        [Required]
        public string Fullname { get; set; } = null!;

        public int? RoleId { get; set; } = 1;

        public DateTime? DateOfBirth { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? AvatarUrl { get; set; }

        public virtual ICollection<Result> Results { get; set; } = new List<Result>();

        public virtual Role? Role { get; set; }
        public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
    }
}
