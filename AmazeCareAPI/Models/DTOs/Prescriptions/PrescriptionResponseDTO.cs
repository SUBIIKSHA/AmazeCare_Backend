namespace AmazeCareAPI.Models.DTOs
{
    public class PrescriptionResponseDTO
    {
        public int PrescriptionID { get; set; }
        public int RecordID { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string PatternCode { get; set; } = string.Empty; 
        public string DosageTiming { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int Days { get; set; }
        public string? Notes { get; set; }
        public DateTime PrescribedDate { get; set; }
    }
}
