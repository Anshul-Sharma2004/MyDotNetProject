namespace RoleBasedJWTMVC.DTOs;

public class RegisterDto
{
    public int RegisterID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; }
     public string PhoneNumber { get; set; }
}
