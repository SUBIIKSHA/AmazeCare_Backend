namespace AmazeCareAPI.Models.DTOs
{
    public class AppointmentSearchResponseDTO
    {
        public int AppointmentID { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime AppointmentDateTime { get; set; }
        public string? Symptoms { get; set; }
        public string? VisitReason { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
