using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmazeCareAPI.Interfaces
{
    public interface IDoctorService
    {
        Task<ICollection<Doctor>> GetAllDoctors();
        Task<Doctor> GetDoctorById(int id);
        Task<Doctor> AddDoctor(AddDoctorRequestDTO request);
        Task<Doctor> UpdateDoctor(int id, UpdateDoctorRequestDTO request);
        Task<bool> DeleteDoctor(int id);
        Task<DoctorMasterResponseDTO> GetDataForAddingDoctors();
        Task<PaginatedDoctorResponseDTO> SearchDoctors(DoctorSearchRequestDTO request);
        Task<ICollection<Doctor>> GetDoctorsBySpecialization(int specializationId);
        Task<IEnumerable<DoctorSearchResponseDTO>> GetDoctorsByStatusAsync(int statusId);
        Task<bool> DeactivateDoctorAsync(int doctorId);

    }
}
