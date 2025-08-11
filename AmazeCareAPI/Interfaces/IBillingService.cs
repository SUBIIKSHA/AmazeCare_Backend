
using AmazeCareAPI.Models.DTOs;

namespace AmazeCareAPI.Services
{
    public interface IBillingService
    {
        Task<IEnumerable<BillingResponseDTO>> GetAllAsync();
        Task<BillingResponseDTO> GetByIdAsync(int billingId);
        Task<IEnumerable<BillingResponseDTO>> GetByAppointmentIdAsync(int appointmentId);
        Task<IEnumerable<BillingResponseDTO>> GetByStatusIdAsync(int statusId);
        Task<IEnumerable<BillingResponseDTO>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<BillingResponseDTO> CreateAsync(BillingCreateDTO billingDTO);
    }
}
