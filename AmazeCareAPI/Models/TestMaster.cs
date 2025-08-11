namespace AmazeCareAPI.Models
{
    public class TestMaster
    {
        public int TestID { get; set; }
        public string TestName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public string? Instructions { get; set; }
    }

}
