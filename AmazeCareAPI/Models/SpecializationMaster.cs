using System.Numerics;

namespace AmazeCareAPI.Models
{
    public class SpecializationMaster
    {
        public int SpecializationID { get; set; }
        public string SpecializationName { get; set; } = string.Empty;

        public ICollection<Doctor> Doctors { get; set; }
    }

}
