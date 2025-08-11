namespace AmazeCareAPI.Models.DTOs
{
    public class UpdateDoctorRequestDTO
    {
        public string? Name { get; set; }
        public int? SpecializationID { get; set; }
        public int? Experience { get; set; }
        public int? QualificationID { get; set; }
        public string? Designation { get; set; }
        public string? ContactNumber { get; set; }
    }
}
