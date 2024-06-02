using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASPFinalProject.Models
{
    public class SubmitHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int QuestionId { get; set; }
        [Required]
        public string answer { get; set; }
        public virtual Question? Question { get; set; }
        public virtual User? user { get; set; }
    }
}
