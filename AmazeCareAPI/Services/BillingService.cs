using AmazeCareAPI.Contexts;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AmazeCareAPI.Services
{
    public class BillingService : IBillingService
    {
        private readonly BillingRepositoryDB _repository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public BillingService(BillingRepositoryDB repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
            _context = repository.Context;
        }

        public async Task<IEnumerable<BillingResponseDTO>> GetAllAsync()
        {
            var billings = await _repository.GetAll();
            return _mapper.Map<IEnumerable<BillingResponseDTO>>(billings);
        }

        public async Task<BillingResponseDTO> GetByIdAsync(int billingId)
        {
            var billing = await _repository.GetById(billingId);
            return _mapper.Map<BillingResponseDTO>(billing);
        }

        public async Task<IEnumerable<BillingResponseDTO>> GetByAppointmentIdAsync(int appointmentId)
        {
            var billings = await _repository.GetByAppointmentIdAsync(appointmentId);
            return _mapper.Map<IEnumerable<BillingResponseDTO>>(billings);
        }

        public async Task<IEnumerable<BillingResponseDTO>> GetByStatusIdAsync(int statusId)
        {
            var billings = await _repository.GetByStatusIdAsync(statusId);
            return _mapper.Map<IEnumerable<BillingResponseDTO>>(billings);
        }

        public async Task<IEnumerable<BillingResponseDTO>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var billings = await _repository.GetByDateRangeAsync(fromDate, toDate);
            return _mapper.Map<IEnumerable<BillingResponseDTO>>(billings);
        }

        public async Task<BillingResponseDTO> CreateAsync(BillingCreateDTO billingDTO)
        {
            decimal totalAmount = await CalculateTotalAmountAsync(billingDTO.AppointmentID);
            var billing = new Billing
            {
                AppointmentID = billingDTO.AppointmentID,
                StatusID = billingDTO.StatusID,
                BillingDate = billingDTO.BillingDate,
                TotalAmount = totalAmount,
                PatientID = await GetPatientIdByAppointment(billingDTO.AppointmentID)
            };

            var result = await _repository.Add(billing);
            var savedWithBill = await _context.Billings
                .Include(r => r.BillingStatus)
                .FirstOrDefaultAsync(r => r.StatusID == result.StatusID);

            return _mapper.Map<BillingResponseDTO>(result);
        }

        private async Task<int> GetPatientIdByAppointment(int appointmentId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentID == appointmentId);

            if (appointment == null)
                throw new NoSuchEntityException("Appointment not found.");

            return appointment.PatientID;
        }

        private async Task<decimal> CalculateTotalAmountAsync(int appointmentId)
{
    var record = await _context.MedicalRecords
        .Include(r => r.Prescriptions)
            .ThenInclude(p => p.Medicine)
        .Include(r => r.Prescriptions)
            .ThenInclude(p => p.RecommendedTests)
                .ThenInclude(rt => rt.Test)
        .FirstOrDefaultAsync(r => r.AppointmentID == appointmentId);

    if (record == null)
        throw new NoSuchEntityException("Medical record not found for this appointment.");

    var prescriptions = record.Prescriptions ?? new List<Prescription>();

    if (!prescriptions.Any())
        throw new Exception("No prescriptions found to generate billing.");

    decimal medicineTotal = prescriptions
        .Where(p => p.Medicine != null)
        .Sum(p => (p.Quantity > 0 ? p.Quantity : 1) * p.Medicine.Price);

    decimal testTotal = prescriptions
        .SelectMany(p => p.RecommendedTests ?? new List<RecommendedTest>())
        .Where(rt => rt.Test != null)
        .Sum(rt => rt.Test.Price);

    return medicineTotal + testTotal;
}

    }
}
