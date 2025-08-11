namespace AmazeCareAPI.Models
{
    public class AppointmentStatusMaster
    {
        public int StatusID { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public ICollection<Appointment> Appointments { get; set; }
    }
}
