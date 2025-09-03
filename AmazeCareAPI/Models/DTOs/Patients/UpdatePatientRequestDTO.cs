using System.ComponentModel.DataAnnotations;

public class UpdatePatientRequestDTO
{
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
    public string? FullName { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DOB { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "GenderID must be greater than 0.")]
    public int? GenderID { get; set; }

    [Phone(ErrorMessage = "Invalid contact number.")]
    [StringLength(15, ErrorMessage = "Contact number cannot exceed 15 digits.")]
    public string? ContactNumber { get; set; }

    [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
    public string? Address { get; set; }
     public string BloodGroup { get; set; } = string.Empty;
}
