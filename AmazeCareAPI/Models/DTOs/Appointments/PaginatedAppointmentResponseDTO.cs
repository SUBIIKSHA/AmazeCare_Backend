namespace AmazeCareAPI.Models.DTOs
{
    public class PaginatedAppointmentResponseDTO
    {
        public List<AppointmentSearchResponseDTO> Appointments { get; set; }
        public int TotalNumberOfRecords { get; set; }
        public int PageNumber { get; set; }
        public ErrorObjectDTO? Error { get; set; }
    }
}
