using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models.DTOs
{
    public class RecommendedTestCreateDTO
    {
        [Required]
        public int PrescriptionID { get; set; }

        [Required]
        public int TestID { get; set; }
    }
}
