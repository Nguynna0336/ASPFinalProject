using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPFinalProject.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int TestId { get; set; }

        [Required]
        public int UserId { get; set; }

        public float? Score { get; set; }
        public DateTime? SubmitAt { get; set; }

        [ForeignKey("TestId")]
        public virtual Test Test { get; set; }

        [ForeignKey("UserId")]
        public virtual User  User { get; set; }
    }
}
