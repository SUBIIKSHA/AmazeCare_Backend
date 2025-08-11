namespace AmazeCareAPI.Models.DTOs
{
    public class AddPatientResponseDTO
    {
        public int PatientID { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime DOB { get; set; }
        public int Age => CalculateAge(DOB);

        private int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            int age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;
            return age;
        }
        public string Message { get; set; } = "Patient registered successfully";
    }
}
