namespace AmazeCareAPI.Models
{
    public class DoctorStatusMaster
    {
        public int StatusID { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public ICollection<Doctor> Doctors { get; set; }
    }
}
