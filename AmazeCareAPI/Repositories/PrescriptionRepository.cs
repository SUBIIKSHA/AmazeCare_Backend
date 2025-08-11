using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;

namespace AmazeCareAPI.Repositories
{
    public class PrescriptionRepository : Repository<int, Prescription>
    {
        public async override Task<Prescription> GetById(int key)
        {
            var item = list.FirstOrDefault(p => p.PrescriptionID == key);
            if (item == null)
                throw new NoSuchEntityException();
            return item;
        }
    }
}
