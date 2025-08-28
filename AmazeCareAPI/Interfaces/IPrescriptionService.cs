using AmazeCareAPI.Models.DTOs;

namespace AmazeCareAPI.Services
{
    public interface IPrescriptionService
    {
        Task<PrescriptionResponseDTO> AddPrescriptionAsync(PrescriptionCreateDTO dto);
        Task<IEnumerable<PrescriptionResponseDTO>> GetAllPrescriptionsAsync();

        Task<IEnumerable<PrescriptionResponseDTO>> GetPrescriptionsByRecordIdAsync(int recordId);

    }
}
