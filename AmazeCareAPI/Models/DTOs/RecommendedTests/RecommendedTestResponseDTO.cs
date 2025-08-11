namespace AmazeCareAPI.Models.DTOs
{
    public class RecommendedTestResponseDTO
    {
        public int RecommendedTestID { get; set; }

        public int TestID { get; set; }

        public string TestName { get; set; } = string.Empty;
    }

}
