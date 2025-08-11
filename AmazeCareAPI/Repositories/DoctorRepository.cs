using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;

namespace AmazeCareAPI.Repositories
{
    public class DoctorRepository : Repository<int, Doctor>
    {
        public override async Task<Doctor> GetById(int key)
        {
            var doctor = list.FirstOrDefault(d => d.DoctorID == key);
            if (doctor == null)
                throw new NoSuchEntityException();
            return doctor;
        }
    }
}
