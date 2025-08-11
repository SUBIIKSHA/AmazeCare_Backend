using AmazeCareAPI.Contexts;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AmazeCareAPI.Repositories
{
    public class MedicalRecordRepositoryDB : RepositoryDB<int, MedicalRecord>
    {
        public MedicalRecordRepositoryDB(ApplicationDbContext context) : base(context) { }

        public async override Task<IEnumerable<MedicalRecord>> GetAll()
        {
            return await _context.MedicalRecords
                .Include(r => r.Appointment)
                .Include(r => r.Prescriptions)
                    .ThenInclude(p => p.Medicine)
                .Include(r => r.Prescriptions)
                    .ThenInclude(p => p.DosagePattern)
                .ToListAsync();
        }

        public async override Task<MedicalRecord> GetById(int key)
        {
            var record = await _context.MedicalRecords
                .Include(r => r.Appointment)
                .Include(r => r.Prescriptions)
                    .ThenInclude(p => p.Medicine)
                .Include(r => r.Prescriptions)
                    .ThenInclude(p => p.DosagePattern)
                .FirstOrDefaultAsync(r => r.RecordID == key);

            if (record == null)
                throw new NoSuchEntityException();

            return record;
        }

        public async Task<IEnumerable<MedicalRecord>> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _context.MedicalRecords
                .Where(r => r.AppointmentID == appointmentId)
                .Include(r => r.Appointment)
                .Include(r => r.Prescriptions)
                    .ThenInclude(p => p.Medicine)
                .Include(r => r.Prescriptions)
                    .ThenInclude(p => p.DosagePattern)
                .ToListAsync();
        }

        public async Task<bool> SoftDeleteMedicalRecord(int recordId)
        {
            var record = await _context.MedicalRecords
                .Include(r => r.Prescriptions)
                .FirstOrDefaultAsync(r => r.RecordID == recordId);

            if (record == null)
                return false;

            record.Status = "Deleted";

            foreach (var prescription in record.Prescriptions)
                prescription.Status = "Deleted";

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<MedicalRecord>> GetByStatusAsync(string status)
        {
            return await _context.MedicalRecords
                .Include(r => r.Appointment)
                .Include(r => r.Prescriptions)
                .Where(r => r.Status == status)
                .ToListAsync();
        }


    }
}
