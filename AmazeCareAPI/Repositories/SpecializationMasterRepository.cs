using AmazeCareAPI.Contexts;
using AmazeCareAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazeCareAPI.Repositories
{
    public class SpecializationMasterRepository : RepositoryDB<int, SpecializationMaster>
    {
        public SpecializationMasterRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<SpecializationMaster>> GetAll()
        {
            return await _context.Specializations.ToListAsync();
        }

        public override async Task<SpecializationMaster> GetById(int key)
        {
            return await _context.Specializations.FirstOrDefaultAsync(s => s.SpecializationID == key);
        }
    }
}
