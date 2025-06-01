using System.ComponentModel.DataAnnotations;

namespace RoleBasedJWTMVC.Models
{
    public class Team
    {
        [Key]
        public int Id { get; set; }
        public int TeamId { get; set; }
        public string MemberName { get; set; }
        public string MemberEmail { get; set; }
        public string Specialization { get; set; }
    }
}
