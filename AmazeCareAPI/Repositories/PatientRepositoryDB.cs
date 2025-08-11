using AmazeCareAPI.Contexts;
using AmazeCareAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AmazeCareAPI.Repositories
{
    public class PatientRepositoryDB : RepositoryDB<int, Patient>
    {
        public PatientRepositoryDB(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Patient>> GetAll()
        {
            return await _context.Patients.ToListAsync();
        }

        public override async Task<Patient> GetById(int key)
        {
            return await _context.Patients.SingleOrDefaultAsync(p => p.PatientID == key);
        }

        public async virtual Task<IEnumerable<Patient>> GetPatientsByStatusAsync(int statusId)
        {
            return await _context.Patients
                .Include(p => p.Gender)
                .Where(p => p.StatusID == statusId)
                .ToListAsync();
        }

        public async virtual Task<bool> DeactivatePatientAsync(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null) return false;

            patient.StatusID = 2;

            var user = await _context.Users.FindAsync(patient.UserName);
            if (user != null)
            {
                user.Status = "InActive"; 
                _context.Users.Update(user);
            }

            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();
            return true;
        }



    }
}
