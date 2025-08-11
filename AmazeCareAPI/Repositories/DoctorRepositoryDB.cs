using AmazeCareAPI.Contexts;
using AmazeCareAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazeCareAPI.Repositories
{
    public class DoctorRepositoryDB : RepositoryDB<int, Doctor>
    {
        public DoctorRepositoryDB(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Doctor>> GetAll()
        {
            return await _context.Doctors.ToListAsync();
        }

        public override async Task<Doctor> GetById(int key)
        {
            return await _context.Doctors.SingleOrDefaultAsync(d => d.DoctorID == key);
        }

        public virtual async Task<IEnumerable<Doctor>> GetDoctorsByStatusAsync(int statusId)
        {
            return await _context.Doctors
                .Include(d => d.Specialization)
                .Include(d => d.Qualification)
                .Where(d => d.StatusID == statusId)
                .ToListAsync();
        }

        public virtual async Task<bool> DeactivateDoctorAsync(int doctorId)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return false;

            doctor.StatusID = 2;

            var user = await _context.Users.FindAsync(doctor.UserName);
            if (user != null)
            {
                user.Status = "Inactive";
                _context.Users.Update(user);
            }

            _context.Doctors.Update(doctor);
            await _context.SaveChangesAsync();
            return true;
        }


    }
}
