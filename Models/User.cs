namespace RoleBasedJWTMVC.Models
{
    public class User
    {
        public int Id { get; set; }
        public int RegisterID { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; }

        public string PhoneNumber { get; set; }   

           
    }
}
