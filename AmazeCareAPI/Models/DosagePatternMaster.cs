namespace AmazeCareAPI.Models
{
    public class DosagePatternMaster
    {
        public int PatternID { get; set; }
        public string PatternCode { get; set; } = string.Empty;
        public string Timing { get; set; } = string.Empty;

        public ICollection<Prescription> Prescriptions { get; set; }
    }

}
