namespace AmazeCareAPI.Models.DTOs
{
    public class PaginatedDoctorResponseDTO
    {
        public List<DoctorSearchResponseDTO> Doctors { get; set; }
        public int PageNumber { get; set; }
        public int TotalNumberOfRecords { get; set; }
        public ErrorObjectDTO? Error { get; set; }
    }
}
