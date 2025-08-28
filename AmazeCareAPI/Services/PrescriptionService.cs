using AmazeCareAPI.Contexts;
using AmazeCareAPI.Interfaces;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AmazeCareAPI.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IRepository<int, Prescription> _prescriptionRepository;
        private readonly ApplicationDbContext _context;

        private readonly IMapper _mapper;

        public PrescriptionService(IRepository<int, Prescription> prescriptionRepository, IMapper mapper, ApplicationDbContext applicationDbContext)
        {
            _prescriptionRepository = prescriptionRepository;
            _mapper = mapper;
            _context = applicationDbContext;
        }

        public async Task<PrescriptionResponseDTO> AddPrescriptionAsync(PrescriptionCreateDTO dto)
        {
            var prescription = _mapper.Map<Prescription>(dto);
            prescription.PrescribedDate = DateTime.UtcNow;

            var result = await _prescriptionRepository.Add(prescription);

            var fullPrescription = await _context.Prescriptions
                .Include(p => p.Medicine)
                .Include(p => p.DosagePattern)
                .FirstOrDefaultAsync(p => p.PrescriptionID == result.PrescriptionID);

            return _mapper.Map<PrescriptionResponseDTO>(fullPrescription);
        }
        public async Task<IEnumerable<PrescriptionResponseDTO>> GetAllPrescriptionsAsync()
        {
            var prescriptions = await _prescriptionRepository.GetAll();
            return _mapper.Map<IEnumerable<PrescriptionResponseDTO>>(prescriptions);
        }

        public async Task<IEnumerable<PrescriptionResponseDTO>> GetPrescriptionsByRecordIdAsync(int recordId)
        {
            if (_prescriptionRepository is PrescriptionRepositoryDB repoDB)
            {
                var prescriptions = await repoDB.GetByRecordIdAsync(recordId);
                return _mapper.Map<IEnumerable<PrescriptionResponseDTO>>(prescriptions);
            }
            else
            {
                var all = await _prescriptionRepository.GetAll();
                var filtered = all.Where(p => p.RecordID == recordId);
                return _mapper.Map<IEnumerable<PrescriptionResponseDTO>>(filtered);
            }
        }
    }
}
