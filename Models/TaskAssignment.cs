using System;

namespace RoleBasedJWTMVC.Models
{
    public class TaskAssignment
    {
        public int Id { get; set; }

        // public int TeamId { get; set; }
        public string Email { get; set; }
        public int EmployeeId { get; set; }
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public string Technology { get; set; }        
        public DateTime AssignedDate { get; set; }
        public DateTime DueDate { get; set; }
        public string AssignedBy { get; set; }

    }
}
