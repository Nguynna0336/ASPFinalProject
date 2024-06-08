using System.ComponentModel.DataAnnotations;

namespace ASPFinalProject.DTOs.Question
{
    public class QuestionDTO
    {
        [Required]
        public string Description { get; set; } = null!;

        public string? OptionA { get; set; }

        public string? OptionB { get; set; }

        public string? OptionC { get; set; }

        public string? OptionD { get; set; }

        [Required]
        public int Answer { get; set; } = 1;

    }
}
