using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models.DTOs
{
    public class BillingCreateDTO
    {
        [Required]
        public int AppointmentID { get; set; }

        [Required]
        public int StatusID { get; set; }

        [Required]
        public DateTime BillingDate { get; set; }
    }


}
