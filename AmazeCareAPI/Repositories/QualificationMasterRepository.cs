using AmazeCareAPI.Contexts;
using AmazeCareAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazeCareAPI.Repositories
{
    public class QualificationMasterRepository : RepositoryDB<int, QualificationMaster>
    {
        public QualificationMasterRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<QualificationMaster>> GetAll()
        {
            return await _context.Qualifications.ToListAsync();
        }

        public override async Task<QualificationMaster> GetById(int key)
        {
            return await _context.Qualifications.FirstOrDefaultAsync(q => q.QualificationID == key);
        }
    }
}
