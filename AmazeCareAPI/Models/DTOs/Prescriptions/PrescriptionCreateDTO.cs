using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models.DTOs
{
    public class PrescriptionCreateDTO
    {
        [Required]
        public int RecordID { get; set; }

        [Required]
        public int MedicineID { get; set; }

        [Required]
        public int PatternID { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Range(1, 365, ErrorMessage = "Days must be between 1 and 365.")]
        public int Days { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
