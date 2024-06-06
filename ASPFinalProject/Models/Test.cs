using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPFinalProject.Models
{
    public class Test
    {
        [Key]
        public int TestId { get; set; }

        public string TestTitle { get; set; }

        public string? Description { get; set; }

        public bool IsOpen { get; set; } = true;

        public int AuthorId { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public string? Password { get; set; }

        public int? Time { get; set; }

        public int NumberOfQuestion { get; set; }

        [ForeignKey("AuthorId")]
        public virtual User Author { get; set; }

        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<Result> Results { get; set; } = new List<Result>();
    }
}
