using AmazeCareAPI.Contexts;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Helpers;
using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Repositories;
using AmazeCareAPI.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace AmazeCareAPI.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IRepository<int, Doctor> _doctorRepository;
        private readonly IRepository<int, SpecializationMaster> _specializationRepository;
        private readonly IRepository<int, QualificationMaster> _qualificationRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public DoctorService(
            IRepository<int, Doctor> doctorRepository,
            IRepository<int, SpecializationMaster> specializationRepository,
            IRepository<int, QualificationMaster> qualificationRepository,
            IRepository<string, User> userRepository,
            IMapper mapper, ApplicationDbContext applicationDbContext)
        {
            _doctorRepository = doctorRepository;
            _specializationRepository = specializationRepository;
            _qualificationRepository = qualificationRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _context = applicationDbContext;
        }

        public async Task<ICollection<Doctor>> GetAllDoctors()
        {
            var doctors = await _doctorRepository.GetAll();
            return doctors.ToList();
        }

        public async Task<Doctor> GetDoctorById(int id)
        {
            var doctor = await _doctorRepository.GetById(id);
            if (doctor == null)
                throw new NoSuchEntityException("Doctor not found");

            return _mapper.Map<Doctor>(doctor);
        }

        public async Task<Doctor> AddDoctor(AddDoctorRequestDTO request)
        {
            var existingUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == request.Username);

            if (existingUser != null)
                throw new Exception($"Username '{request.Username}' already exists.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = SecurityHelper.PopulateUserObject(request.Username, request.Email, request.Password);
                user.RoleID = request.RoleID;
                user.CreatedAt = DateTime.UtcNow;

                var addedUser = await _userRepository.Add(user);

                var doctor = _mapper.Map<Doctor>(request);
                doctor.UserName = addedUser.UserName;

                var addedDoctor = await _doctorRepository.Add(doctor);

                addedUser.DoctorID = addedDoctor.DoctorID;
                await _userRepository.Update(addedUser.UserName, addedUser);

                await transaction.CommitAsync();
                return addedDoctor;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw new Exception("Doctor registration failed");
            }
        }

        public async Task<Doctor> UpdateDoctor(int id, UpdateDoctorRequestDTO request)
        {
            var doctor = await _doctorRepository.GetById(id);
            if (doctor == null)
                throw new NoSuchEntityException($"Doctor with ID {id} not found");

            if (!string.IsNullOrWhiteSpace(request.Name))
                doctor.Name = request.Name;

            if (request.SpecializationID.HasValue)
            {
                var spec = await _specializationRepository.GetById(request.SpecializationID.Value);
                if (spec == null)
                    throw new Exception("Invalid SpecializationID");
                doctor.SpecializationID = request.SpecializationID.Value;
            }

            if (request.QualificationID.HasValue)
            {
                var qual = await _qualificationRepository.GetById(request.QualificationID.Value);
                if (qual == null)
                    throw new Exception("Invalid QualificationID");
                doctor.QualificationID = request.QualificationID.Value;
            }

            if (request.Experience.HasValue)
                doctor.Experience = request.Experience.Value;

            if (!string.IsNullOrWhiteSpace(request.Designation))
                doctor.Designation = request.Designation;

            if (!string.IsNullOrWhiteSpace(request.ContactNumber))
                doctor.ContactNumber = request.ContactNumber;

            return await _doctorRepository.Update(id, doctor);
        }

        public async Task<bool> DeleteDoctor(int id)
        {
            var doctor = await _doctorRepository.GetById(id);
            if (doctor == null)
                throw new NoSuchEntityException("Doctor not found");

            await _doctorRepository.Delete(id);
            return true;
        }

        public async Task<DoctorMasterResponseDTO> GetDataForAddingDoctors()
        {
            var specializations = await _specializationRepository.GetAll();
            var qualifications = await _qualificationRepository.GetAll();

            return new DoctorMasterResponseDTO
            {
                Specializations = specializations.ToList(),
                Qualifications = qualifications.ToList()
            };
        }

        public async Task<ICollection<Doctor>> GetDoctorsBySpecialization(int specializationId)
        {
            var doctors = await _doctorRepository.GetAll();
            return doctors.Where(d => d.SpecializationID == specializationId).ToList();
        }

        public async Task<PaginatedDoctorResponseDTO> SearchDoctors(DoctorSearchRequestDTO request)
        {
            var doctors = await _doctorRepository.GetAll();

            if (doctors == null || !doctors.Any())
                throw new NotFoundException("doctors");

            if (doctors.Any() && request.Name != null)
                doctors = await SearchByName(doctors, request.Name);

            if (doctors.Any() && request.SpecializationIds != null && request.SpecializationIds.Any())
                doctors = await FilterBySpecializations(doctors, request.SpecializationIds);

            if (doctors.Any() && request.QualificationIds != null && request.QualificationIds.Any())
                doctors = await FilterByQualifications(doctors, request.QualificationIds);

            if (doctors.Any() && request.ExperienceRange != null)
                doctors = await FilterByExperience(doctors, request.ExperienceRange);

            if (doctors.Any() && request.Sort != 0)
                doctors = SortDoctors(doctors, request.Sort);

            var totalCount = doctors.Count();
            var result = await PopulateSpecializationAndQualification(doctors);

            if (result.Count < request.PageSize)
            {
                return new PaginatedDoctorResponseDTO
                {
                    Doctors = result.ToList(),
                    TotalNumberOfRecords = totalCount,
                    PageNumber = request.PageNumber
                };
            }

            result = result.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();

            return new PaginatedDoctorResponseDTO
            {
                Doctors = result.ToList(),
                TotalNumberOfRecords = totalCount,
                PageNumber = request.PageNumber
            };
        }
        private async Task<IEnumerable<Doctor>> SearchByName(IEnumerable<Doctor> doctors, string name)
        {
            name = name.ToLower();
            return doctors.Where(d => d.Name.ToLower().Contains(name)).ToList();
        }

        private async Task<IEnumerable<Doctor>> FilterBySpecializations(IEnumerable<Doctor> doctors, List<int> specializationIds)
        {
            return doctors.Where(d => specializationIds.Contains(d.SpecializationID)).ToList();
        }

        private async Task<IEnumerable<Doctor>> FilterByQualifications(IEnumerable<Doctor> doctors, List<int> qualificationIds)
        {
            return doctors.Where(d => qualificationIds.Contains(d.QualificationID)).ToList();
        }

        private async Task<IEnumerable<Doctor>> FilterByExperience(IEnumerable<Doctor> doctors, SearchRange<int> experienceRange)
        {
            return doctors.Where(d => d.Experience >= experienceRange.MinValue && d.Experience <= experienceRange.MaxValue).ToList();
        }

        private IEnumerable<Doctor> SortDoctors(IEnumerable<Doctor> doctors, int sort)
        {
            switch (sort)
            {
                case 1: return doctors.OrderBy(d => d.Name);
                case -1: return doctors.OrderByDescending(d => d.Name);
                case 2: return doctors.OrderBy(d => d.Experience);
                case -2: return doctors.OrderByDescending(d => d.Experience);
                case 3: return doctors.OrderBy(d => d.SpecializationID);
                case -3: return doctors.OrderByDescending(d => d.SpecializationID);
                default: return doctors;
            }
        }

        private async Task<ICollection<DoctorSearchResponseDTO>> PopulateSpecializationAndQualification(IEnumerable<Doctor> doctors)
        {
            var result = new List<DoctorSearchResponseDTO>();
            foreach (var doctor in doctors)
            {
                var dto = _mapper.Map<DoctorSearchResponseDTO>(doctor);
                dto.SpecializationName = (await _specializationRepository.GetById(doctor.SpecializationID))?.SpecializationName ?? "N/A";
                dto.QualificationName = (await _qualificationRepository.GetById(doctor.QualificationID))?.QualificationName ?? "N/A";
                result.Add(dto);
            }
            return result;
        }

        public async Task<bool> DeactivateDoctorAsync(int doctorId)
        {
            var repo = _doctorRepository as DoctorRepositoryDB;
            if (repo == null) throw new Exception("Invalid repo");
            return await repo.DeactivateDoctorAsync(doctorId);
        }

        public async Task<IEnumerable<DoctorSearchResponseDTO>> GetDoctorsByStatusAsync(int statusId)
        {
            var repo = _doctorRepository as DoctorRepositoryDB;
            if (repo == null) throw new Exception("Invalid repo");
            var doctors = await repo.GetDoctorsByStatusAsync(statusId);
            return _mapper.Map<IEnumerable<DoctorSearchResponseDTO>>(doctors);
        }


    }
}
