using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;  
using RoleBasedJWTMVC.Data; 

namespace RoleBasedJWTMVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
            
        public IActionResult AdminDashboard()
        {
            // Count users with role "Admin"
            int adminCount = _context.Users.Count(u => u.Role == "Admin");

            // Get current logged-in user's name or username
            string userName = User.Identity.Name;

            // Optionally fetch full name from DB if needed
            var currentUser = _context.Users.FirstOrDefault(u => u.Email == userName);
            string nameToDisplay = currentUser?.Name ?? "Admin";

            // Pass data to view
            ViewData["AdminCount"] = adminCount;
            ViewData["AdminName"] = nameToDisplay;

            return View("Views/Dashboard/AdminDashboard.cshtml");
        }


        // Single Chat() method only, update the view path if needed
        public IActionResult Chat()
        {
            return View("/Views/Admin/Chat.cshtml");
        }

        [Authorize(Roles = "Employee")]
        public IActionResult EmployeeDashboard()
        {
            // Return the view normally
            return View("Views/Dashboard/EmployeeDashboard.cshtml");
        }
    }
}
