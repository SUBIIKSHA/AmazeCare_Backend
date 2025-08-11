namespace AmazeCareAPI.Models.DTOs
{
    public class ErrorObjectDTO
    {
        public int ErrorNumber { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
