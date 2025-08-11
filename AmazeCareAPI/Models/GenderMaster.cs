namespace AmazeCareAPI.Models
{
    public class GenderMaster
    {
        public int GenderID { get; set; }
        public string GenderName { get; set; } = string.Empty;

        public ICollection<Patient> Patients { get; set; }
    }
}
