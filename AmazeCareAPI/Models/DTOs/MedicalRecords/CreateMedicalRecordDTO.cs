using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models.DTOs
{
    public class CreateMedicalRecordDTO
    {
        [Required(ErrorMessage = "AppointmentID is required.")]
        public int AppointmentID { get; set; }

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
