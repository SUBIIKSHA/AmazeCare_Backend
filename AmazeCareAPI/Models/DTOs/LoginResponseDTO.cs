namespace AmazeCareAPI.Models.DTOs
{
    public class LoginResponseDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public int? PatientID { get; set; }
        public int? DoctorID { get; set; }
    }
}
