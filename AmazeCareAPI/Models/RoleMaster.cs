namespace AmazeCareAPI.Models
{
    public class RoleMaster
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;

        public ICollection<User> Users { get; set; }
    }
}
