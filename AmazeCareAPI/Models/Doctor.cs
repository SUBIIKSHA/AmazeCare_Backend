namespace AmazeCareAPI.Models
{
    public class Doctor
    {
        public int DoctorID { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SpecializationID { get; set; }
        public int Experience { get; set; }
        public int QualificationID { get; set; }
        public string? Designation { get; set; }
        public string? ContactNumber { get; set; }
        public int StatusID { get; set; } 

        public DoctorStatusMaster Status { get; set; }

        public User User { get; set; }
        public SpecializationMaster Specialization { get; set; }
        public QualificationMaster Qualification { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<MedicalRecord> MedicalRecords { get; set; }
    }

}
