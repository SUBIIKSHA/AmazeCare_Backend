using AmazeCareAPI.Contexts;
using AmazeCareAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AmazeCareAPI.Repositories
{
    public class AppointmentRepositoryDB : RepositoryDB<int, Appointment>
    {
        public AppointmentRepositoryDB(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Appointment>> GetAll()
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Status)
                .ToListAsync();
        }

        public override async Task<Appointment> GetById(int key)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.AppointmentID == key);
        }
    }
}
