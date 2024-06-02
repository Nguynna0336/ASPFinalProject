using System.ComponentModel.DataAnnotations;

namespace ASPFinalProject.DTOs.Test
{
    public class TestDTO
    {
        [Required]
        public required string TestTitle { get; set; }

        public string? Description { get; set; }

        public bool IsOpen { get; set; } = true;

        public string? Password { get; set; }

        public int? Time { get; set; }

        public int NumberOfQuestion { get; set; }
    }
}
