namespace AmazeCareAPI.Models
{
    public class MedicineMaster
    {
        public int MedicineID { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public ICollection<Prescription> Prescriptions { get; set; }
    }

}
