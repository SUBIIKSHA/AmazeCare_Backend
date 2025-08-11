using AmazeCareAPI.Models.DTOs;

namespace AmazeCareAPI.Services
{
    public interface IMedicalRecordService
    {
        Task<IEnumerable<MedicalRecordDTO>> GetAllAsync();
        Task<MedicalRecordDTO> GetByIdAsync(int id);
        Task<MedicalRecordDTO> CreateAsync(CreateMedicalRecordDTO dto);
        Task<MedicalRecordDTO> UpdateAsync(int id, UpdateMedicalRecordDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<MedicalRecordDTO>> GetByAppointmentIdAsync(int appointmentId);
        Task<bool> DeleteMedicalRecord(int id);
        Task<IEnumerable<MedicalRecordDTO>> GetMedicalRecordsByStatusAsync(string status);

    }
}
