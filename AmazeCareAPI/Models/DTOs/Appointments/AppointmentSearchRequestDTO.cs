using System;
using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models.DTOs
{
    public class AppointmentSearchRequestDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "DoctorID must be greater than 0.")]
        public int? DoctorID { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "PatientID must be greater than 0.")]
        public int? PatientID { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "StatusID must be greater than 0.")]
        public int? StatusID { get; set; }

        public SearchRange<DateTime>? DateRange { get; set; }

        [Range(0, 10, ErrorMessage = "Sort value is not valid.")]
        public int Sort { get; set; } = 0;

        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be at least 1.")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 10;
    }
}
