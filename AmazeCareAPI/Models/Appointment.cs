namespace AmazeCareAPI.Models
{
    public class Appointment
    {
        public int AppointmentID { get; set; }
        public int PatientID { get; set; }
        public int DoctorID { get; set; }
        public int StatusID { get; set; }
        public string? Symptoms { get; set; }
        public string? VisitReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime AppointmentDateTime { get; set; }

        public Patient Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public AppointmentStatusMaster Status { get; set; }

        public Billing? Billing { get; set; }
    }

}
