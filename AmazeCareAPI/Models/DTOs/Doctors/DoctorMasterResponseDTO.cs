namespace AmazeCareAPI.Models.DTOs
{
    public class DoctorMasterResponseDTO
    {
        public ICollection<SpecializationMaster> Specializations { get; set; }
        public ICollection<QualificationMaster> Qualifications { get; set; }
    }
}
