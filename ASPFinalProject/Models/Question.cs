using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace ASPFinalProject.Models
{
    public class Question
    {
        [Key]
        public int QuestionsId { get; set; }

        public int TestId { get; set; }

        public string Description { get; set; } = null!;

        public string? OptionA { get; set; }

        public string? OptionB { get; set; }

        public string? OptionC { get; set; }

        public string? OptionD { get; set; }

        public sbyte Answer { get; set; }

        public int? DocumentId { get; set; }

        public int? DocumentPage { get; set; }

        public virtual Document? Document { get; set; }

        public virtual Test Test { get; set; } = null!;
    }
}
