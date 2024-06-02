using System.ComponentModel.DataAnnotations;

namespace ASPFinalProject.DTOs.Submit
{
    public class SubmitDTO
    {
        [Required]
        public int questionId {  get; set; }
        [Required]
        public string answer {  get; set; }
    }
}
