namespace AmazeCareAPI.Models.DTOs
{
    public class AppointmentSearchResponseDTO
    {
        public int AppointmentID { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime ScheduledDateTime { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }
}
