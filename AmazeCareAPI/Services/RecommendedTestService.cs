using AmazeCareAPI.Contexts;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;
using AmazeCareAPI.Models.DTOs;
using AmazeCareAPI.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AmazeCareAPI.Services
{
    public class RecommendedTestService : IRecommendedTestService
    {
        private readonly RecommendedTestRepositoryDB _repository;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RecommendedTestService(RecommendedTestRepositoryDB repository, ApplicationDbContext context, IMapper mapper)
        {
            _repository = repository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RecommendedTestResponseDTO>> GetAllAsync()
        {
            var entities = await _repository.GetAll();
            return _mapper.Map<IEnumerable<RecommendedTestResponseDTO>>(entities);
        }

        public async Task<RecommendedTestResponseDTO> GetByIdAsync(int id)
        {
            var entity = await _repository.GetById(id);
            return _mapper.Map<RecommendedTestResponseDTO>(entity);
        }

        public async Task<IEnumerable<RecommendedTestResponseDTO>> GetByPrescriptionIdAsync(int recordId)
        {
            var tests = await _repository.GetByPrescriptionIdAsync(recordId);
            return _mapper.Map<IEnumerable<RecommendedTestResponseDTO>>(tests);
        }

        public async Task<RecommendedTestResponseDTO> AddAsync(RecommendedTestCreateDTO dto)
        {
            var recordExists = await _context.Prescriptions.AnyAsync(r => r.PrescriptionID == dto.PrescriptionID);
            if (!recordExists)
                throw new NoSuchEntityException($"MedicalRecord with ID {dto.PrescriptionID} not found");

            var testExists = await _context.Tests.AnyAsync(t => t.TestID == dto.TestID);
            if (!testExists)
                throw new NoSuchEntityException($"Test with ID {dto.TestID} not found");

            var entity = _mapper.Map<RecommendedTest>(dto);
            var saved = await _repository.Add(entity);

            var savedWithTest = await _context.RecommendedTests
                .Include(r => r.Test)
                .FirstOrDefaultAsync(r => r.RecommendedTestID == saved.RecommendedTestID);

            return _mapper.Map<RecommendedTestResponseDTO>(savedWithTest!);
        }


        public async Task<RecommendedTestResponseDTO> DeleteAsync(int id)
        {
            var deleted = await _repository.Delete(id);
            if (deleted == null)
                throw new NoSuchEntityException($"RecommendedTest with ID {id} not found");

            return _mapper.Map<RecommendedTestResponseDTO>(deleted);
        }
    }
}
