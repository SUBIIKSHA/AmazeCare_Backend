using System.Numerics;

namespace AmazeCareAPI.Models
{
    public class QualificationMaster
    {
        public int QualificationID { get; set; }
        public string QualificationName { get; set; } = string.Empty;

        public ICollection<Doctor> Doctors { get; set; }
    }

}
