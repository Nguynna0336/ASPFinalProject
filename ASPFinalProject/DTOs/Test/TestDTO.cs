using System.ComponentModel.DataAnnotations;

namespace ASPFinalProject.DTOs.Test
{
    public class TestDTO
    {
        [Required]
        public string TestTitle { get; set; }

        public string? Description { get; set; }

        public bool IsOpen { get; set; } = false;

        public string? Password { get; set; }

        public int? Time { get; set; }

        public int NumberOfQuestion { get; set; }
    }
}
