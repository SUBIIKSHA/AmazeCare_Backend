namespace AmazeCareAPI.Models.DTOs
{
    public class BillingResponseDTO
    {
        public int BillingID { get; set; }
        public int AppointmentID { get; set; }
        public decimal TotalAmount { get; set; }
        public int StatusID { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime BillingDate { get; set; }
    }

}
