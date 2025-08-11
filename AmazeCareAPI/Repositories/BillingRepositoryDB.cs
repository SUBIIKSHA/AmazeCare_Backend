using AmazeCareAPI.Contexts;
using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AmazeCareAPI.Repositories
{
    public class BillingRepositoryDB : RepositoryDB<int, Billing>
    {
        public ApplicationDbContext Context => _context;
        public BillingRepositoryDB(ApplicationDbContext context) : base(context) { }

        public override async Task<IEnumerable<Billing>> GetAll()
        {
            return await _context.Billings
                .Include(b => b.Appointment)
                .Include(b => b.BillingStatus)
                .AsNoTracking()
                .ToListAsync();
        }

        public override async Task<Billing> GetById(int key)
        {
            var billing = await _context.Billings
                .Include(b => b.Appointment)
                .Include(b => b.BillingStatus)
                .FirstOrDefaultAsync(b => b.BillingID == key);

            if (billing == null)
                throw new NoSuchEntityException();

            return billing;
        }

        public async Task<IEnumerable<Billing>> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _context.Billings
                .Include(b => b.Appointment)
                .Include(b => b.BillingStatus)
                .Where(b => b.AppointmentID == appointmentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Billing>> GetByStatusIdAsync(int statusId)
        {
            return await _context.Billings
                .Include(b => b.Appointment)
                .Include(b => b.BillingStatus)
                .Where(b => b.StatusID == statusId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Billing>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Billings
                .Include(b => b.Appointment)
                .Include(b => b.BillingStatus)
                .Where(b => b.BillingDate.Date >= fromDate.Date && b.BillingDate.Date <= toDate.Date)
                .ToListAsync();
        }
    }

}
