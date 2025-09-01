namespace AmazeCareAPI.Models.DTOs
{
    public class RecommendedTestResponseDTO
    {
        public int RecommendedTestID { get; set; }

        public int TestID { get; set; }
        public int PrescriptionID { get; set; }

        public string TestName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public string Instructions { get; set; } = string.Empty;
    }

}
