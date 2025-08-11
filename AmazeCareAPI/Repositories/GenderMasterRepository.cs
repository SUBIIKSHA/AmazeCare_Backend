using AmazeCareAPI.Contexts;
using AmazeCareAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazeCareAPI.Repositories
{
    public class GenderMasterRepository : RepositoryDB<int, GenderMaster>
    {
        public GenderMasterRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<GenderMaster>> GetAll()
        {
            return await _context.Genders.Include(g => g.Patients).ToListAsync();
        }

        public override async Task<GenderMaster> GetById(int key)
        {
            return await _context.Genders.FirstOrDefaultAsync(g => g.GenderID == key);
        }
    }
}
