using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RoleBasedJWTMVC.Data;
using RoleBasedJWTMVC.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
// using System.Security.Claims;

namespace RoleBasedJWTMVC.Controllers
{
    [Authorize]
    public class DashboardDataController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /DashboardData/EmployeeTasks
        [Authorize(Roles = "Employee")]
        [HttpGet]
        public async Task<IActionResult> EmployeeTasks()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized();

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (employee == null)
                return NotFound("Employee not found.");

            ViewBag.EmployeeName = employee.Name;
            ViewBag.EmployeeEmail = employee.Email;

            var tasks = await _context.EmployeeTasks
                .Where(t => t.EmployeeId == employee.Id)
                .OrderByDescending(t => t.AssignedDate)
                .ToListAsync();

            return View("EmployeeTasks", tasks);
        }

        // POST: Complete Task
        [HttpPost]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> CompleteTask(int taskId)
        {
            var task = await _context.EmployeeTasks.FindAsync(taskId);
            if (task == null)
                return NotFound();

            task.Status = "Completed";
            _context.EmployeeTasks.Update(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("EmployeeTasks");
        }

        // POST: Delete Task
        [HttpPost]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var task = await _context.EmployeeTasks.FindAsync(taskId);
            if (task == null)
                return NotFound();

            _context.EmployeeTasks.Remove(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("EmployeeTasks");
        }
        public string GetUserId(HubConnectionContext connection)
        {
            // Use the user's email as the identifier
            return connection.User?.FindFirst(ClaimTypes.Email)?.Value;
        }

        // GET: /DashboardData/Chat
        [HttpGet]
        public IActionResult Chat()
        {
            var users = _context.Employees.ToList();
            return View("/Views/DashboardData/Chat.cshtml", users);
        }

        // GET: /DashboardData/ChatBox/{empId}
        [HttpGet]
        public IActionResult ChatBox(int empId)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == empId);
            if (employee == null)
                return NotFound();

            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(currentUserEmail))
                return Unauthorized();

            var messages = _context.ChatMessages
                .Where(m =>
                    (m.SenderId == currentUserEmail && m.ReceiverId == employee.Email) ||
                    (m.SenderId == employee.Email && m.ReceiverId == currentUserEmail)
                )
                .OrderBy(m => m.Timestamp)
                .ToList();

            var viewModel = new ChatBoxViewModel
            {
                SenderId = currentUserEmail,
                ReceiverId = employee.Email,
                ReceiverName = employee.Name,
                Messages = messages.Select(m => new Message
                {
                    SenderId = m.SenderId,
                    Text = m.Message
                }).ToList()
            };

            return View(viewModel);
        }
        
        
    }
}
