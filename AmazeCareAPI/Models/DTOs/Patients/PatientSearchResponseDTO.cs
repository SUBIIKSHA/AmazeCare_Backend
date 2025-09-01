using System;

namespace AmazeCareAPI.Models.DTOs
{
    public class PatientSearchResponseDTO
    {
        public int PatientID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public DateTime DOB { get; set; }
        public string BloodGroup { get; set; } = string.Empty;
        public string GenderName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int StatusID { get; set; }
    }
}
