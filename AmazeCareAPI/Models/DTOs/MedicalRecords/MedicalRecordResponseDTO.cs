namespace AmazeCareAPI.Models.DTOs
{
    public class MedicalRecordDTO
    {
        public int RecordID { get; set; }
        public int patientID { get; set; }
        public int doctorID { get; set; }

        public int AppointmentID { get; set; }
        public string? Symptoms { get; set; }
        public string? PhysicalExamination { get; set; }
        public string? TreatmentPlan { get; set; }
        public string? Diagnosis { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<PrescriptionResponseDTO>? Prescriptions { get; set; }
        public List<RecommendedTestResponseDTO>? RecommendedTests { get; set; }
    }
}
