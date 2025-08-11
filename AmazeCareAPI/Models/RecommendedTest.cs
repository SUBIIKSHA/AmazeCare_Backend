namespace AmazeCareAPI.Models
{
    public class RecommendedTest
    {
        public int RecommendedTestID { get; set; }
        public int TestID { get; set; }
        public int PrescriptionID { get; set; }
        public TestMaster Test { get; set; }
        public Prescription Prescription { get; set; }

        public string Status { get; set; } = "Active";
    }
}
