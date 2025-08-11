using AmazeCareAPI.Exceptions;
using AmazeCareAPI.Models;

namespace AmazeCareAPI.Repositories
{
    public class RecommendedTestRepository : Repository<int, RecommendedTest>
    {
        public async override Task<RecommendedTest> GetById(int key)
        {
            var item = list.FirstOrDefault(r => r.RecommendedTestID == key);
            if (item == null)
                throw new NoSuchEntityException();
            return item;
        }
    }
}
