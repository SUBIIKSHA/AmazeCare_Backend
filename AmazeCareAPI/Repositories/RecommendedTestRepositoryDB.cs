using AmazeCareAPI.Contexts;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AmazeCareAPI.Repositories
{
    public class RecommendedTestRepositoryDB : RepositoryDB<int, RecommendedTest>
    {
        public RecommendedTestRepositoryDB(ApplicationDbContext context) : base(context) { }

        public async override Task<IEnumerable<RecommendedTest>> GetAll()
        {
            return await _context.RecommendedTests
                .Include(r => r.Test)
                .Include(r => r.Prescription)
                .ToListAsync();
        }

        public async override Task<RecommendedTest> GetById(int key)
        {
            var test = await _context.RecommendedTests
                .Include(r => r.Test)
                .Include(r => r.Prescription)
                .FirstOrDefaultAsync(r => r.RecommendedTestID == key);

            if (test == null)
                throw new NoSuchEntityException();

            return test;
        }

        public async Task<IEnumerable<RecommendedTest>> GetByPrescriptionIdAsync(int prescriptionId)
        {
            return await _context.RecommendedTests
                .Include(r => r.Test)
                .Include(r => r.Prescription)
                .Where(r => r.PrescriptionID == prescriptionId)
                .ToListAsync();
        }

    }
}
