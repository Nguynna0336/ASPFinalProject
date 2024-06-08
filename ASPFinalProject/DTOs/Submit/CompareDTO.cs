using ASPFinalProject.Models;

namespace ASPFinalProject.DTOs.Submit
{
    public class CompareDTO
    {
        public Models.Question question {  get; set; }
        public Models.SubmitHistory? history { get; set; }
    }
}
