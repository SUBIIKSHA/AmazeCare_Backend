using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;

namespace AmazeCareAPI.Repositories
{
    public class MedicalRecordRepository : Repository<int, MedicalRecord>
    {
        public async override Task<MedicalRecord> GetById(int key)
        {
            var item = list.FirstOrDefault(r => r.RecordID == key);
            if (item == null)
                throw new NoSuchEntityException();
            return item;
        }
    }
}
