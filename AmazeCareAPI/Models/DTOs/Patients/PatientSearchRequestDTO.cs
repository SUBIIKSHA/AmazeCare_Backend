using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models.DTOs
{
    public class PatientSearchRequestDTO
    {
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string? FullName { get; set; }

        public List<int>? GenderIds { get; set; }

        public SearchRange<DateTime>? DOBRange { get; set; }

        [Range(0, 5, ErrorMessage = "Sort must be a valid value.")]
        public int Sort { get; set; } = 0;

        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be at least 1.")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 10;
    }
}
