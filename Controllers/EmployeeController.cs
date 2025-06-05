using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RoleBasedJWTMVC.Data;
using RoleBasedJWTMVC.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

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

        [Authorize(Roles = "Employee")]
        [HttpGet]
        public async Task<IActionResult> EmployeeTasks()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized();

            var employee = await _context.Employees
            // .Include(t => t.AssignedBy);
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Email == userEmail);

            if (employee == null)
                return NotFound("Employee not found.");

            ViewBag.EmployeeName = employee.Name;
            ViewBag.EmployeeEmail = employee.Email;

            var tasks = await _context.EmployeeTasks
                .Where(t => t.EmployeeId == employee.Id)
                .Include(t => t.AssignedBy)
                .OrderByDescending(t => t.AssignedDate)
                .ToListAsync();

            return View("EmployeeTasks", tasks);
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
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

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var task = await _context.EmployeeTasks.FindAsync(taskId);
            if (task == null)
                return NotFound();

            _context.EmployeeTasks.Remove(task);
            await _context.SaveChangesAsync();

            return RedirectToAction("EmployeeTasks");
        }

        [HttpGet]
        public IActionResult Chat()
        {
            var employees = _context.Employees.ToList();
            return View("/Views/DashboardData/Chat.cshtml", employees);
        }


        [HttpGet]
        public async Task<IActionResult> ChatBox(int EmpId)
        {
            var senderEmail = User.FindFirstValue(ClaimTypes.Email);
            var sender = await _context.Employees.FirstOrDefaultAsync(e => e.Email == senderEmail);
            if (sender == null)
                return NotFound();

            // Try finding the receiver in Employees
            var receiver = await _context.Employees.FindAsync(EmpId);

            // If not found in Employees, check in Admins
            if (receiver == null)
            {
                var admin = await _context.Admins.FindAsync(EmpId); // assuming Admin ID is same type
                if (admin == null) return NotFound();

                var messages = await _context.ChatMessages
                    .Where(m => (m.SenderId == sender.Email && m.ReceiverId == admin.Email) ||
                                (m.SenderId == admin.Email && m.ReceiverId == sender.Email))
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();

                var viewModel = new ChatBoxViewModel
                {
                    SenderId = sender.Email,
                    SenderName = sender.Name,
                    ReceiverId = admin.Email,
                    ReceiverName = admin.Name,
                    Messages = messages
                };

                return View("/Views/DashboardData/ChatBox.cshtml", viewModel);
            }

            // Chat with employee (original logic)
            var employeeMessages = await _context.ChatMessages
                .Where(m => (m.SenderId == sender.Email && m.ReceiverId == receiver.Email) ||
                            (m.SenderId == receiver.Email && m.ReceiverId == sender.Email))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            var employeeViewModel = new ChatBoxViewModel
            {
                SenderId = sender.Email,
                SenderName = sender.Name,
                ReceiverId = receiver.Email,
                ReceiverName = receiver.Name,
                Messages = employeeMessages
            };

            return View("/Views/DashboardData/ChatBox.cshtml", employeeViewModel);
        }

[HttpGet("ChatBoxWithAdmin")]
public async Task<IActionResult> ChatBoxWithAdmin(string adminEmail)
{
    var senderEmail = User.FindFirstValue(ClaimTypes.Email);
    var sender = await _context.Employees.FirstOrDefaultAsync(e => e.Email == senderEmail);
    var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == adminEmail);

    if (sender == null || admin == null)
        return NotFound();

    var messages = await _context.ChatMessages
        .Where(m => (m.SenderId == sender.Email && m.ReceiverId == admin.Email) ||
                    (m.SenderId == admin.Email && m.ReceiverId == sender.Email))
        .OrderBy(m => m.Timestamp)
        .ToListAsync();

    var viewModel = new ChatBoxViewModel
    {
        SenderId = sender.Email,
        SenderName = sender.Name,
        ReceiverId = admin.Email,
        ReceiverName = admin.Name,
        Messages = messages
    };

    return View("/Views/DashboardData/ChatBox.cshtml", viewModel);
}



    }
}
