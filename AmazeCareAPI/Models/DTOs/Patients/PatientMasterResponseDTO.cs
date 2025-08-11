using System.Collections.Generic;

namespace AmazeCareAPI.Models.DTOs
{
    public class PatientMasterResponseDTO
    {
        public ICollection<GenderMaster> Genders { get; set; }
    }
}
