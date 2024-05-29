using System.ComponentModel.DataAnnotations;

namespace ASPFinalProject.Models
{
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }

        public string? Title { get; set; }

        public string? Url { get; set; }

        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
