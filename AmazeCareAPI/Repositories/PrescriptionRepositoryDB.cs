using AmazeCareAPI.Contexts;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AmazeCareAPI.Repositories
{
    public class PrescriptionRepositoryDB : RepositoryDB<int, Prescription>
    {
        public PrescriptionRepositoryDB(ApplicationDbContext context) : base(context) { }

        public async override Task<IEnumerable<Prescription>> GetAll()
        {
            return await _context.Prescriptions
                .Include(p => p.Medicine)
                .Include(p => p.DosagePattern)
                .ToListAsync();
        }

        public async override Task<Prescription> GetById(int key)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Medicine)
                .Include(p => p.DosagePattern)
                .FirstOrDefaultAsync(p => p.PrescriptionID == key);

            if (prescription == null)
                throw new NoSuchEntityException();

            return prescription;
        }

        public async Task<IEnumerable<Prescription>> GetByRecordIdAsync(int recordId)
        {
            return await _context.Prescriptions
                .Include(p => p.Medicine)
                .Include(p => p.DosagePattern)
                .Where(p => p.RecordID == recordId)
                .ToListAsync();
        }
    }
}
