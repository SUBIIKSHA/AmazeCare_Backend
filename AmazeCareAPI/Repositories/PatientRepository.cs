using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;

namespace AmazeCareAPI.Repositories
{
    public class PatientRepository : Repository<int, Patient>
    {
        public async override Task<Patient> GetById(int key)
        {
            var patient = list.FirstOrDefault(p => p.PatientID == key);
            if (patient == null)
                throw new NoSuchEntityException();
            return patient;
        }
    }
}
