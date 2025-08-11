using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;

namespace AmazeCareAPI.Repositories
{
    public class BillingRepository : Repository<int, Billing>
    {
        public override async Task<Billing> GetById(int key)
        {
            var item = list.FirstOrDefault(b => b.BillingID == key);
            if (item == null)
                throw new NoSuchEntityException();
            return item;
        }

        public async Task<IEnumerable<Billing>> GetByAppointmentIdAsync(int appointmentId)
        {
            return list.Where(b => b.AppointmentID == appointmentId);
        }

        public async Task<IEnumerable<Billing>> GetByStatusIdAsync(int statusId)
        {
            return list.Where(b => b.StatusID == statusId);
        }

        public async Task<IEnumerable<Billing>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return list.Where(b => b.BillingDate.Date >= fromDate.Date && b.BillingDate.Date <= toDate.Date);
        }
    }
}
