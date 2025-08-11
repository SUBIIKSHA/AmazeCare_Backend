using System;
using System.ComponentModel.DataAnnotations;

namespace AmazeCareAPI.Models.DTOs
{
    public class AddPatientRequestDTO
    {
        [Required]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "GenderID must be greater than 0.")]
        public int GenderID { get; set; }

        [Required]
        [MaxLength(3)]
        [RegularExpression("^(A|B|AB|O)[+-]$")]
        public string? BloodGroup { get; set; }


        [Required]
        [Phone(ErrorMessage = "Invalid contact number.")]
        [StringLength(15, ErrorMessage = "Contact number cannot exceed 15 digits.")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "RoleID must be greater than 0.")]
        public int RoleID { get; set; }
        [Required]
        public int statusID { get; set; }
    }
}
