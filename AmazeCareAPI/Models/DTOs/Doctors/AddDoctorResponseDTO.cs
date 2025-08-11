namespace AmazeCareAPI.Models.DTOs
{
    public class AddDoctorResponseDTO
    {
        public int DoctorID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Message { get; set; } = "Doctor registered successfully";
    }
}
