namespace AmazeCareAPI.Models
{
    public class User
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public byte[] Password { get; set; }
        public byte[] HashKey { get; set; }
        public int RoleID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Active";
        public RoleMaster Role { get; set; }
        public int? DoctorID { get; set; }
        public Patient? Patient { get; set; }
        public int? PatientID { get; set; }
        public Doctor? Doctor { get; set; }
    }
}
