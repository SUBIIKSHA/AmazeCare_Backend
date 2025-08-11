namespace AmazeCareAPI.Models
{
    public class Patient
    {
        public int PatientID { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime DOB { get; set; }
        public int GenderID { get; set; }
        public string BloodGroup { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public int StatusID { get; set; } 

        public PatientStatusMaster Status { get; set; }
        public User User { get; set; }
        public GenderMaster Gender { get; set; }
        
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Billing> Billings { get; set; }
        public ICollection<MedicalRecord> MedicalRecords { get; set; }
    }

}
