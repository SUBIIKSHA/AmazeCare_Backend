
using AmazeCareAPI.Models.DTOs;

namespace AmazeCareAPI.Services
{
    public interface IRecommendedTestService
    {
        Task<IEnumerable<RecommendedTestResponseDTO>> GetAllAsync();
        Task<RecommendedTestResponseDTO> GetByIdAsync(int id);
        Task<IEnumerable<RecommendedTestResponseDTO>> GetByPrescriptionIdAsync(int prescriptionId);
        Task<RecommendedTestResponseDTO> AddAsync(RecommendedTestCreateDTO dto);
        Task<RecommendedTestResponseDTO> DeleteAsync(int id);
    }
}
