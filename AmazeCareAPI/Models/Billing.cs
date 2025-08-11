namespace AmazeCareAPI.Models
{
    public class Billing
    {
        public int BillingID { get; set; }
        public int AppointmentID { get; set; }
        public int PatientID { get; set; }
        public int StatusID { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime BillingDate { get; set; }

        public Appointment Appointment { get; set; }
        public Patient Patient { get; set; }
        public BillingStatusMaster BillingStatus { get; set; }
    }
}
