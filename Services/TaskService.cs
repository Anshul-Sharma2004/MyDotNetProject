using System;
using RoleBasedJWTMVC.Data;

namespace RoleBasedJWTMVC.Services
{
    public class TaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AssignTaskToMember(string email, string memberId, string memberName, string taskId)
        {
            // Logic for internal tracking or audit
            Console.WriteLine($"Task {taskId} assigned to {memberName} ({email})");
        }
    }
}
