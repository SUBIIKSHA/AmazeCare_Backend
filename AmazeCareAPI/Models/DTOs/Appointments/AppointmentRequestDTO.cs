using System;
using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models.DTOs
{
    public class AppointmentRequestDTO
    {
        [Required(ErrorMessage = "PatientID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "PatientID must be greater than 0.")]
        public int PatientID { get; set; }

        [Required(ErrorMessage = "DoctorID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "DoctorID must be greater than 0.")]
        public int DoctorID { get; set; }

        [Required(ErrorMessage = "AppointmentDateTime is required.")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDateTime { get; set; }

        [StringLength(500, ErrorMessage = "Symptoms cannot exceed 500 characters.")]
        public string? Symptoms { get; set; }

        [StringLength(500, ErrorMessage = "VisitReason cannot exceed 500 characters.")]
        public string? VisitReason { get; set; }
    }
}
