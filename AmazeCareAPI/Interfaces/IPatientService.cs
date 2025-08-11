using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmazeCareAPI.Interfaces
{
    public interface IPatientService
    {
        Task<ICollection<Patient>> GetAllPatients();
        Task<Patient> GetPatientById(int id);
        Task<Patient> AddPatient(AddPatientRequestDTO request);
        Task<Patient> UpdatePatient(int id, UpdatePatientRequestDTO request);
        Task<bool> DeletePatient(int id);
        Task<PatientMasterResponseDTO> GetDataForAddingPatients();
        Task<PaginatedPatientResponseDTO> SearchPatients(PatientSearchRequestDTO request);
        Task<IEnumerable<PatientSearchResponseDTO>> GetPatientsByStatusAsync(int statusId);
        Task<bool> DeactivatePatientAsync(int patientId);
    }
}
