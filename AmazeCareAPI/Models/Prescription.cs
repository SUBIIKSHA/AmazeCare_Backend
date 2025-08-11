using AmazeCareAPI.Models;

namespace AmazeCareAPI.Models
{
    public class Prescription
    {
        public int PrescriptionID { get; set; }
        public int RecordID { get; set; }
        public int MedicineID { get; set; }
        public int PatternID { get; set; }
        public int Quantity { get; set; } 
        public int Days { get; set; }
        public string? Notes { get; set; }
        public DateTime PrescribedDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Active";
        public MedicalRecord Record { get; set; }
        public MedicineMaster Medicine { get; set; }
        public DosagePatternMaster DosagePattern { get; set; }
        public ICollection<RecommendedTest> RecommendedTests { get; set; }
    }
}
