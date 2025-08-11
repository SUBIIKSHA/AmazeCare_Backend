using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models.DTOs
{
    public class AddDoctorRequestDTO
    {
        [Required]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Phone(ErrorMessage = "Invalid phone number.")]
        [StringLength(15, ErrorMessage = "Contact number cannot exceed 15 digits.")]
        public string ContactNumber { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Designation cannot exceed 100 characters.")]
        public string? Designation { get; set; }

        [Range(0, 60, ErrorMessage = "Experience must be between 0 and 60 years.")]
        public int Experience { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "SpecializationID must be greater than 0.")]
        public int SpecializationID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "QualificationID must be greater than 0.")]
        public int QualificationID { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "RoleID must be greater than 0.")]
        public int RoleID { get; set; }

        [Required]
        public int statusID { get; set; }
    }
}
