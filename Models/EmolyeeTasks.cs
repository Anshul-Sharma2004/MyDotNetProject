using RoleBasedJWTMVC.Models; // or wherever Employee class is located
using System;
using System.ComponentModel.DataAnnotations;

namespace RoleBasedJWTMVC.Models
{
    public class EmployeeTask
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public int EmployeeId { get; set; }  
        [Required]
        [StringLength(200)]
        public string TaskTitle { get; set; }

        
        [StringLength(100)]
        public string Technology { get; set; } 

        [StringLength(1000)]
        public string TaskDescription { get; set; }

        [Required]
        public DateTime AssignedDate { get; set; }

        public DateTime? DueDate { get; set; }

        public Employee Employee { get; set; }  
        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(200)]
       public string AssignedById { get; set; }
       public User AssignedBy { get; set; } 
    }
}
