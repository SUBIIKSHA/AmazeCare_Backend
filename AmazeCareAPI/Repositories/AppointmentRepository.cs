using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;

namespace AmazeCareAPI.Repositories
{
    public class AppointmentRepository : Repository<int, Appointment>
    {
        public async override Task<Appointment> GetById(int key)
        {
            var item = list.FirstOrDefault(a => a.AppointmentID == key);
            if (item == null)
                throw new NoSuchEntityException();
            return item;
        }

        
    }
}
