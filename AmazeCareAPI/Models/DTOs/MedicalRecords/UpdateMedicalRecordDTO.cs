using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models.DTOs
{
    public class UpdateMedicalRecordDTO
    {
        [StringLength(1000)]
        public string? Symptoms { get; set; }

        [StringLength(1000)]
        public string? PhysicalExamination { get; set; }

        [StringLength(1000)]
        public string? TreatmentPlan { get; set; }

        [StringLength(1000)]
        public string? Diagnosis { get; set; }
    }
}
