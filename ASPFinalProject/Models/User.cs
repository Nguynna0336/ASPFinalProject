using MySqlX.XDevAPI.Common;
using static System.Net.Mime.MediaTypeNames;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ASPFinalProject.Models
{
    public class User : IdentityUser<int>
    {
        [Required]
        public string Fullname { get; set; }

        public virtual Role Role { get; set; }

        // Thêm các quan hệ với các entity khác nếu cần thiết
        public virtual ICollection<Result> Results { get; set; } = new List<Result>();
        public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
    }
}
