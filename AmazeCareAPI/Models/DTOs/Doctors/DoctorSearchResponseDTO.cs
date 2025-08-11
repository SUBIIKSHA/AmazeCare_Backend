namespace AmazeCareAPI.Models.DTOs
{
    public class DoctorSearchResponseDTO
    {
        public int DoctorID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Designation { get; set; }
        public string? ContactNumber { get; set; }
        public int Experience { get; set; }

        public string SpecializationName { get; set; } = string.Empty;
        public string QualificationName { get; set; } = string.Empty;
    }
}
