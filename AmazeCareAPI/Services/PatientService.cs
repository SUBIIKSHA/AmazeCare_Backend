using AmazeCareAPI.Contexts;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Helpers;
using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AmazeCareAPI.Services
{
    public class PatientService : IPatientService
    {
        private readonly IRepository<int, Patient> _patientRepository;
        private readonly IRepository<int, GenderMaster> _genderRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public PatientService(
            IRepository<int, Patient> patientRepository,
            IRepository<int, GenderMaster> genderRepository,
            IRepository<string, User> userRepository,
            IMapper mapper, ApplicationDbContext applicationDbContext)
        {
            _patientRepository = patientRepository;
            _genderRepository = genderRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _context = applicationDbContext;
        }

        public async Task<ICollection<Patient>> GetAllPatients()
        {
            var patients = await _patientRepository.GetAll();
            return patients.ToList();
        }

        public async Task<Patient> GetPatientById(int id)
        {
            return await _patientRepository.GetById(id);
        }
        public async Task<IEnumerable<Patient>> GetPatientsByDoctorIdAsync(int doctorId)
        {
            var patients = await _context.Appointments
                .Where(a => a.DoctorID == doctorId)
                .Select(a => a.Patient)
                .Distinct()
                .ToListAsync();

            return _mapper.Map<IEnumerable<Patient>>(patients);
        }

        public async Task<Patient> AddPatient(AddPatientRequestDTO request)
        {
            var userRepo = _userRepository as UserRepository;
            if (userRepo == null)
                throw new Exception("Invalid user repository");

            var existingUser = await userRepo.GetByUsernameAsync(request.Username);
            if (existingUser != null)
                throw new Exception($"Username '{request.Username}' already exists.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = SecurityHelper.PopulateUserObject(request.Username, request.Email, request.Password);
                user.RoleID = request.RoleID;
                user.CreatedAt = DateTime.UtcNow;

                var addedUser = await _userRepository.Add(user);

                var patient = _mapper.Map<Patient>(request);
                patient.UserName = addedUser.UserName;

                var addedPatient = await _patientRepository.Add(patient);

                addedUser.PatientID = addedPatient.PatientID;
                await _userRepository.Update(addedUser.UserName, addedUser);

                await transaction.CommitAsync();
                return addedPatient;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw new Exception("Patient registration failed");
            }
        }

        public async Task<Patient> UpdatePatient(int id, UpdatePatientRequestDTO request)
        {
            var patient = await _patientRepository.GetById(id);
            if (patient == null)
                throw new Exception($"Patient with ID {id} not found");

            if (!string.IsNullOrWhiteSpace(request.FullName))
                patient.FullName = request.FullName;

            if (request.DOB.HasValue)
                patient.DOB = request.DOB.Value;

            if (request.GenderID.HasValue)
                patient.GenderID = request.GenderID.Value;

            if (!string.IsNullOrWhiteSpace(request.ContactNumber))
                patient.ContactNumber = request.ContactNumber;

            if (!string.IsNullOrWhiteSpace(request.Address))
                patient.Address = request.Address;

            return await _patientRepository.Update(id, patient);
        }
        public async Task<bool> DeletePatient(int id)
        {
            var patient = await _patientRepository.GetById(id);
            if (patient == null) return false;

            await _patientRepository.Delete(id);
            return true;
        }

        public async Task<IEnumerable<PatientSearchResponseDTO>> GetPatientsByStatusAsync(int statusId)
        {
            var repo = _patientRepository as PatientRepositoryDB;
            if (repo == null) throw new Exception("Invalid repo");
            var patients = await repo.GetPatientsByStatusAsync(statusId);
            return _mapper.Map<IEnumerable<PatientSearchResponseDTO>>(patients);
        }
        public async Task<bool> DeactivatePatientAsync(int patientId)
        {
            var repo = _patientRepository as PatientRepositoryDB;
            if (repo == null) throw new Exception("Invalid repo");
            return await repo.DeactivatePatientAsync(patientId);
        }


        public async Task<PatientMasterResponseDTO> GetDataForAddingPatients()
        {
            var genders = await _genderRepository.GetAll();
            return new PatientMasterResponseDTO
            {
                Genders = genders.ToList()
            };
        }

        public async Task<PaginatedPatientResponseDTO> SearchPatients(PatientSearchRequestDTO request)
        {
            var patients = await _patientRepository.GetAll();

            if (patients == null || !patients.Any())
                throw new NotFoundException("patients");

            if (!string.IsNullOrEmpty(request.FullName))
                patients = await SearchByName(patients, request.FullName);

            if (request.GenderIds != null && request.GenderIds.Any())
                patients = await FilterByGenders(patients, request.GenderIds);

            if (request.DOBRange != null)
                patients = await FilterByDOB(patients, request.DOBRange);

            if (request.Sort != 0)
                patients = SortPatients(patients, request.Sort);

            var totalCount = patients.Count();
            var result = await PopulateGenderName(patients);

            if (result.Count < request.PageSize)
            {
                return new PaginatedPatientResponseDTO
                {
                    Patients = result.ToList(),
                    TotalNumberOfRecords = totalCount,
                    PageNumber = request.PageNumber
                };
            }

            result = result.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();

            return new PaginatedPatientResponseDTO
            {
                Patients = result.ToList(),
                TotalNumberOfRecords = totalCount,
                PageNumber = request.PageNumber
            };
        }

        private async Task<IEnumerable<Patient>> SearchByName(IEnumerable<Patient> patients, string name)
        {
            name = name.ToLower();
            return patients.Where(p => p.FullName.ToLower().Contains(name)).ToList();
        }

        private async Task<IEnumerable<Patient>> FilterByGenders(IEnumerable<Patient> patients, List<int> genderIds)
        {
            return patients.Where(p => genderIds.Contains(p.GenderID)).ToList();
        }

        private async Task<IEnumerable<Patient>> FilterByDOB(IEnumerable<Patient> patients, SearchRange<DateTime> dobRange)
        {
            return patients.Where(p => p.DOB >= dobRange.MinValue && p.DOB <= dobRange.MaxValue).ToList();
        }

        private IEnumerable<Patient> SortPatients(IEnumerable<Patient> patients, int sort)
        {
            switch (sort)
            {
                case 1: return patients.OrderBy(p => p.FullName);
                case -1: return patients.OrderByDescending(p => p.FullName);
                case 2: return patients.OrderBy(p => p.DOB);
                case -2: return patients.OrderByDescending(p => p.DOB);
                default: return patients;
            }
        }

        private async Task<ICollection<PatientSearchResponseDTO>> PopulateGenderName(IEnumerable<Patient> patients)
        {
            var result = new List<PatientSearchResponseDTO>();
            foreach (var patient in patients)
            {
                var dto = _mapper.Map<PatientSearchResponseDTO>(patient);
                dto.GenderName = (await _genderRepository.GetById(patient.GenderID))?.GenderName ?? "N/A";
                result.Add(dto);
            }
            return result;
        }
    }
}
