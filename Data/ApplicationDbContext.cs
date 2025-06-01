using Microsoft.EntityFrameworkCore;
using RoleBasedJWTMVC.Models;
// using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;


namespace RoleBasedJWTMVC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<EmployeeTask> EmployeeTasks { get; set; }

        public DbSet<Profile> Profiles { get; set; }
        public DbSet<TeamAssign> TeamAssigns { get; set; }
        // public DbSet<AddTeam> AddTeams { get; set; }
        public DbSet<Team> Teams { get; set; }

       public DbSet<LeaveRequest> LeaveRequests { get; set; }

        
        


        // public DbSet<ChatMessage> ChatMessages { get; set; }










    }
}
