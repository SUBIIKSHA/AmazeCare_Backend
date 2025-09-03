using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AutoMapper;

namespace AmazeCareAPI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IRepository<int, Appointment> _appointmentRepo;
        private readonly IRepository<int, Patient> _patientRepo;
        private readonly IRepository<int, Doctor> _doctorRepo;
        private readonly IMapper _mapper;

        public AppointmentService(
            IRepository<int, Appointment> appointmentRepo,
            IRepository<int, Patient> patientRepo,
            IRepository<int, Doctor> doctorRepo,
            IMapper mapper)
        {
            _appointmentRepo = appointmentRepo;
            _patientRepo = patientRepo;
            _doctorRepo = doctorRepo;
            _mapper = mapper;
        }

        private async Task<bool> IsSlotAvailable(int doctorId, DateTime dateTime, int? excludeAppointmentId = null)
        {
            var allAppointments = await _appointmentRepo.GetAll();

            return !allAppointments.Any(a =>
                a.DoctorID == doctorId &&
                a.AppointmentDateTime == dateTime &&
                a.AppointmentID != excludeAppointmentId &&
                a.StatusID != 4 && 
                a.StatusID != 5    
            );
        }

        public async Task<AppointmentResponseDTO> BookAppointment(AppointmentRequestDTO request)
        {
            var patient = await _patientRepo.GetById(request.PatientID)
                ?? throw new NotFoundException("patients");

            var doctor = await _doctorRepo.GetById(request.DoctorID)
                ?? throw new NotFoundException("doctors");

            if (!await IsSlotAvailable(request.DoctorID, request.AppointmentDateTime))
                throw new InvalidOperationException("The doctor is not available at this time slot.");

            var allAppointments = await _appointmentRepo.GetAll();

            var existingAppointment = allAppointments
                .Where(a => a.PatientID == request.PatientID
                         && a.DoctorID == request.DoctorID
                         && a.AppointmentDateTime == request.AppointmentDateTime
                         && (a.StatusID == 1 || a.StatusID == 2))
                .FirstOrDefault();

            if (existingAppointment != null)
                throw new InvalidOperationException("You already have an appointment with this doctor at the same time.");

            var appointment = new Appointment
            {
                PatientID = request.PatientID,
                DoctorID = request.DoctorID,
                AppointmentDateTime = request.AppointmentDateTime,
                Symptoms = request.Symptoms,
                VisitReason = request.VisitReason,
                StatusID = 1, 
                CreatedAt = DateTime.Now
            };

            var saved = await _appointmentRepo.Add(appointment);

            return new AppointmentResponseDTO
            {
                AppointmentID = saved.AppointmentID,
                PatientName = patient.FullName,
                DoctorName = doctor.Name,
                AppointmentDateTime = saved.AppointmentDateTime,
                Symptoms = saved.Symptoms,
                VisitReason = saved.VisitReason,
                Status = "Pending"
            };
        }

        public async Task<Appointment> ApproveAppointment(int appointmentId)
        {
            var appointment = await _appointmentRepo.GetById(appointmentId)
                ?? throw new NotFoundException("appointments");

            appointment.StatusID = 2;

            return await _appointmentRepo.Update(appointmentId, appointment);
        }

        public async Task<Appointment> RescheduleAppointment(int appointmentId, DateTime newDateTime)
        {
            var appointment = await _appointmentRepo.GetById(appointmentId)
                ?? throw new NotFoundException("appointments");

            if (!await IsSlotAvailable(appointment.DoctorID, newDateTime, appointment.AppointmentID))
                throw new InvalidOperationException("The doctor is not available at this time slot.");

            appointment.AppointmentDateTime = newDateTime;
            appointment.StatusID = 2;

            return await _appointmentRepo.Update(appointmentId, appointment);
        }

        public async Task<Appointment> RejectAppointment(int appointmentId)
        {
            var appointment = await _appointmentRepo.GetById(appointmentId)
                ?? throw new NotFoundException("appointments");

            appointment.StatusID = 5; 
            return await _appointmentRepo.Update(appointmentId, appointment);
        }

        public async Task<Appointment> CompleteAppointment(int appointmentId)
        {
            var appointment = await _appointmentRepo.GetById(appointmentId)
                ?? throw new NotFoundException("appointments");

            appointment.StatusID = 3; 
            return await _appointmentRepo.Update(appointmentId, appointment);
        }

        public async Task<Appointment> CancelAppointment(int appointmentId)
        {
            var appointment = await _appointmentRepo.GetById(appointmentId)
                ?? throw new NotFoundException("appointments");

            appointment.StatusID = 4; 
            return await _appointmentRepo.Update(appointmentId, appointment);
        }

        public async Task<IEnumerable<Appointment>> GetAllAppointments()
        {
            var appointments = await _appointmentRepo.GetAll();
            if (appointments == null || !appointments.Any())
                throw new NotFoundException("appointments");

            return appointments;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientId(int patientId)
        {
            var all = await _appointmentRepo.GetAll();
            var filtered = all.Where(a => a.PatientID == patientId).ToList();

            if (!filtered.Any())
                throw new NotFoundException("appointments");

            return filtered;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorId(int doctorId)
        {
            var all = await _appointmentRepo.GetAll();
            return all.Where(a => a.DoctorID == doctorId);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDate(DateTime date)
        {
            var allAppointments = await _appointmentRepo.GetAll();
            return allAppointments
                .Where(a => a.AppointmentDateTime.Date == date.Date)
                .ToList();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByStatus(int statusId)
        {
            var allAppointments = await _appointmentRepo.GetAll();
            return allAppointments
                .Where(a => a.StatusID == statusId)
                .ToList();
        }

        public async Task<PaginatedAppointmentResponseDTO> SearchAppointments(AppointmentSearchRequestDTO request)
        {
            var appointments = await _appointmentRepo.GetAll();

            if (appointments == null || !appointments.Any())
                throw new NotFoundException("appointments");

            if (appointments.Any() && request.DoctorID.HasValue)
                appointments = await SearchByDoctor(appointments, request.DoctorID.Value);

            if (appointments.Any() && request.PatientID.HasValue)
                appointments = await SearchByPatient(appointments, request.PatientID.Value);

            if (appointments.Any() && request.StatusID.HasValue)
                appointments = await FilterByStatus(appointments, request.StatusID.Value);

            if (appointments.Any() && request.DateRange != null)
                appointments = await FilterByDateRange(appointments, request.DateRange);

            if (appointments.Any() && request.Sort != 0)
                appointments = SortAppointments(appointments, request.Sort);

            var totalCount = appointments.Count();
            var result = PopulateDoctorAndPatientDetails(appointments);

            result = result
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PaginatedAppointmentResponseDTO
            {
                Appointments = result.ToList(),
                TotalNumberOfRecords = totalCount,
                PageNumber = request.PageNumber
            };
        }

        private async Task<IEnumerable<Appointment>> SearchByDoctor(IEnumerable<Appointment> appointments, int doctorId)
        {
            return appointments.Where(a => a.DoctorID == doctorId).ToList();
        }

        private async Task<IEnumerable<Appointment>> SearchByPatient(IEnumerable<Appointment> appointments, int patientId)
        {
            return appointments.Where(a => a.PatientID == patientId).ToList();
        }

        private async Task<IEnumerable<Appointment>> FilterByStatus(IEnumerable<Appointment> appointments, int statusId)
        {
            return appointments.Where(a => a.StatusID == statusId).ToList();
        }

        private async Task<IEnumerable<Appointment>> FilterByDateRange(IEnumerable<Appointment> appointments, SearchRange<DateTime> range)
        {
            return appointments.Where(a => a.AppointmentDateTime >= range.MinValue &&
                                            a.AppointmentDateTime <= range.MaxValue).ToList();
        }

        private IEnumerable<Appointment> SortAppointments(IEnumerable<Appointment> appointments, int sort)
        {
            switch (sort)
            {
                case 1: return appointments.OrderBy(a => a.AppointmentDateTime);
                case -1: return appointments.OrderByDescending(a => a.AppointmentDateTime);
                case 2: return appointments.OrderBy(a => a.StatusID);
                case -2: return appointments.OrderByDescending(a => a.StatusID);
                case 3: return appointments.OrderBy(a => a.DoctorID);
                case -3: return appointments.OrderByDescending(a => a.DoctorID);
                default: return appointments;
            }
        }

        private ICollection<AppointmentSearchResponseDTO> PopulateDoctorAndPatientDetails(IEnumerable<Appointment> appointments)
        {
            return appointments.Select(appointment =>
            {
                var dto = _mapper.Map<AppointmentSearchResponseDTO>(appointment);
                dto.DoctorName = appointment.Doctor?.Name ?? "N/A";
                dto.PatientName = appointment.Patient?.FullName ?? "N/A";
                dto.Status = appointment.Status?.StatusName ?? "N/A";
                return dto;
            }).ToList();
        }
    }
}
