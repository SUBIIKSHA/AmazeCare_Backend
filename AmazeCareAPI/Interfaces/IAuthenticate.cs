using AmazeCareAPI.Models.DTOs;
using System.Threading.Tasks;

namespace AmazeCareAPI.Interfaces
{
    public interface IAuthenticate
    {
        Task<AddDoctorResponseDTO> RegisterDoctor(AddDoctorRequestDTO doctor);

        Task<AddPatientResponseDTO> RegisterPatient(AddPatientRequestDTO patient);

        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequest);
    }
}
