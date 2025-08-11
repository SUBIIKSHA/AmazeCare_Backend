using System.Collections.Generic;

namespace AmazeCareAPI.Models.DTOs
{
    public class PaginatedPatientResponseDTO
    {
        public List<PatientSearchResponseDTO> Patients { get; set; }
        public int PageNumber { get; set; }
        public int TotalNumberOfRecords { get; set; }
        public ErrorObjectDTO? Error { get; set; }
    }
}
