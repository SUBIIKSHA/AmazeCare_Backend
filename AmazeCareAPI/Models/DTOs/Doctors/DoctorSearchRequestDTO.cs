using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models.DTOs
{
    public class SearchRange<T>
    {
        public T MinValue { get; set; }
        public T MaxValue { get; set; }
    }

    public class DoctorSearchRequestDTO
    {
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string? Name { get; set; }

        public List<int>? SpecializationIds { get; set; }

        public List<int>? QualificationIds { get; set; }

        public SearchRange<int>? ExperienceRange { get; set; }

        [Range(0, 5, ErrorMessage = "Sort must be a valid value between 0 and 5.")]
        public int Sort { get; set; } = 0;

        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be at least 1.")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 10;
    }

}
