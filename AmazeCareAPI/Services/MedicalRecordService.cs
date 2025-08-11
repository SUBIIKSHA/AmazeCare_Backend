using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Repositories;
using AutoMapper;

namespace AmazeCareAPI.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IRepository<int, MedicalRecord> _medicalRecordRepository;
        private readonly IRepository<int, Appointment> _appointmentRepository;

        private readonly IMapper _mapper;

        public MedicalRecordService(
            IRepository<int, MedicalRecord> medicalRecordRepository,
            IRepository<int, Appointment> appointmentRepository,
            IMapper mapper)
        {
            _medicalRecordRepository = medicalRecordRepository;
            _appointmentRepository = appointmentRepository;
            _mapper = mapper;
        }


        public async Task<MedicalRecordDTO> CreateAsync(CreateMedicalRecordDTO dto)
        {
            var appointment = await _appointmentRepository.GetById(dto.AppointmentID);
            if (appointment == null)
                throw new NoSuchEntityException($"Appointment with ID {dto.AppointmentID} not found");

            var record = _mapper.Map<MedicalRecord>(dto);

            record.DoctorID = appointment.DoctorID;
            record.PatientID = appointment.PatientID;
            record.CreatedAt = DateTime.UtcNow;
            record.Status = "Active";

            var result = await _medicalRecordRepository.Add(record);
            return _mapper.Map<MedicalRecordDTO>(result);
        }

        public async Task<IEnumerable<MedicalRecordDTO>> GetAllAsync()
        {
            var records = await _medicalRecordRepository.GetAll();
            return _mapper.Map<IEnumerable<MedicalRecordDTO>>(records);
        }

        public async Task<MedicalRecordDTO> GetByIdAsync(int id)
        {
            try
            {
                var record = await _medicalRecordRepository.GetById(id);
                return _mapper.Map<MedicalRecordDTO>(record);
            }
            catch(NoSuchEntityException)
            {
                return null;
            }
            
        }

        public async Task<MedicalRecordDTO> UpdateAsync(int id, UpdateMedicalRecordDTO dto)
        {
            var record = await _medicalRecordRepository.GetById(id);

            _mapper.Map(dto, record);
            var updated = await _medicalRecordRepository.Update(id, record);

            return _mapper.Map<MedicalRecordDTO>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var deleted = await _medicalRecordRepository.Delete(id);
            return deleted != null;
        }

        public async Task<IEnumerable<MedicalRecordDTO>> GetByAppointmentIdAsync(int appointmentId)
        {
            if (_medicalRecordRepository is MedicalRecordRepositoryDB repoDB)
            {
                var records = await repoDB.GetByAppointmentIdAsync(appointmentId);
                return _mapper.Map<IEnumerable<MedicalRecordDTO>>(records);
            }
            else
            {
                var all = await _medicalRecordRepository.GetAll();
                var filtered = all.Where(r => r.AppointmentID == appointmentId);
                return _mapper.Map<IEnumerable<MedicalRecordDTO>>(filtered);
            }
        }

        public async Task<bool> DeleteMedicalRecord(int recordId)
        {
            var repo = _medicalRecordRepository as MedicalRecordRepositoryDB;
            if (repo == null) throw new Exception("Invalid repo");
            return await repo.SoftDeleteMedicalRecord(recordId);
        }

        public async Task<IEnumerable<MedicalRecordDTO>> GetMedicalRecordsByStatusAsync(string status)
        {
            var repo = _medicalRecordRepository as MedicalRecordRepositoryDB;
            if (repo == null) throw new Exception("Invalid repo");
            var records = await repo.GetByStatusAsync(status);
            return _mapper.Map<IEnumerable<MedicalRecordDTO>>(records);
        }


    }
}
