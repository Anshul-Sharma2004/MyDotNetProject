using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoleBasedJWTMVC.Models
{
    public class TeamAssign
    {
        [Key]
        public string MemberId { get; set; }

        public int Id { get; set; }
        public int TeamId { get; set; }   // Foreign key to Team
        public string Email { get; set; }
        public string MemberName { get; set; }
        public string Specialization { get; set; }

        public string TaskTitle { get; set; }
        public string TaskTypes { get; set; }
        public string Technology { get; set; }
        public string TaskDescription { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime AssignedDate { get; set; }
        public string AssignedBy { get; set; }
        

         [ForeignKey("TeamId")]
        public Team Team { get; set; }
    }
}
