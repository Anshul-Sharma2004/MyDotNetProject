using System;
using System.ComponentModel.DataAnnotations;

namespace RoleBasedJWTMVC.Models
{

    public enum LeaveStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Accepted = 3

    }
    public class LeaveRequest
    {
        public int Id { get; set; }

        [Required]
        public string EmployeeName { get; set; }

        [Required, EmailAddress]
        public string EmployeeEmail { get; set; }

        [Required]
        public string Reason { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        public DateTime RequestDate { get; set; } = DateTime.Now;
    }
}
