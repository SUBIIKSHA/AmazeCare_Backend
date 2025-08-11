namespace AmazeCareAPI.Models
{
    public class PatientStatusMaster
    {
        public int StatusID { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public ICollection<Patient> Patients { get; set; }
    }
}
