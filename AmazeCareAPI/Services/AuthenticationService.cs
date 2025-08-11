using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Helpers;
using AutoMapper;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AmazeCareAPI.Services
{
    public class AuthenticationService : IAuthenticate
    {
        private readonly IRepository<int, Doctor> _doctorRepository;
        private readonly IRepository<int, Patient> _patientRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AuthenticationService(
            IRepository<int, Doctor> doctorRepository,
            IRepository<int, Patient> patientRepository,
            IRepository<string, User> userRepository,
            ITokenService tokenService,
            IMapper mapper)
        {
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<AddDoctorResponseDTO> RegisterDoctor(AddDoctorRequestDTO doctorDto)
        {
            var user = SecurityHelper.PopulateUserObject(doctorDto.Username, doctorDto.Email, doctorDto.Password);
            user.RoleID = doctorDto.RoleID;
            user.CreatedAt = DateTime.UtcNow;

            await _userRepository.Add(user);

            var newDoctor = _mapper.Map<Doctor>(doctorDto);
            newDoctor.UserName = user.UserName;
            newDoctor = await _doctorRepository.Add(newDoctor);
            user.DoctorID = newDoctor.DoctorID;
            await _userRepository.Update(user.UserName, user);

            return new AddDoctorResponseDTO
            {
                DoctorID = newDoctor.DoctorID,
                Username = user.UserName,
                Message = "Doctor registered successfully"
            };
        }

        public async Task<AddPatientResponseDTO> RegisterPatient(AddPatientRequestDTO patientDto)
        {
            var user = SecurityHelper.PopulateUserObject(patientDto.Username, patientDto.Email, patientDto.Password);
            user.RoleID = patientDto.RoleID;
            user.CreatedAt = DateTime.UtcNow;

            await _userRepository.Add(user);

            var newPatient = _mapper.Map<Patient>(patientDto);
            newPatient.UserName = user.UserName;
            newPatient = await _patientRepository.Add(newPatient);

            user.PatientID = newPatient.PatientID;
            await _userRepository.Update(user.UserName, user);

            return new AddPatientResponseDTO
            {
                PatientID = newPatient.PatientID,
                Username = user.UserName,
                DOB = newPatient.DOB,
                Message = "Patient registered successfully"
            };
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequest)
        {
            var user = await _userRepository.GetById(loginRequest.Username);
            if (user == null)
                throw new NoSuchEntityException();

            HMACSHA256 hmac = new HMACSHA256(user.HashKey);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginRequest.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.Password[i])
                    throw new NoSuchEntityException();
            }

            var token = await _tokenService.GenerateToken(new TokenUser
            {
                Username = user.UserName,
                Role = user.Role.RoleName
            });

            return new LoginResponseDTO
            {
                Username = user.UserName,
                Role = user.Role.RoleName,
                Token = token
            };
        }
    }
}
