using System.ComponentModel.DataAnnotations;

namespace ASPFinalProject.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int? TestId { get; set; }

        [Required]
        public int? UserId { get; set; }

        public int? Score { get; set; }
        public DateTime? SubmitAt { get; set; }

        public virtual Test? Test { get; set; }

        public virtual User? User { get; set; }
    }
}
