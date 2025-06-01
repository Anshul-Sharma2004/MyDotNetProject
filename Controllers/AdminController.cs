using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoleBasedJWTMVC.Data;
using RoleBasedJWTMVC.Models;
using RoleBasedJWTMVC.Services;

namespace RoleBasedJWTMVC.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly MemberService _memberService;
        private readonly TaskService _taskService;

        public AdminController(ApplicationDbContext context, EmailService emailService, MemberService memberService, TaskService taskService)
        {
            _context = context;
            _emailService = emailService;
            _memberService = memberService;
            _taskService = taskService;
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View("/Views/Admin/TaskAssigner.cshtml");
        }

        [HttpGet]
        public IActionResult TaskAssigner()
        {
            ViewBag.AdminName = User.Identity?.Name ?? "Admin";
            return View("/Views/Admin/TaskAssigner.cshtml");
        }

        [HttpGet]
        public IActionResult GetEmployeeByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required");

            var employee = _context.Employees.FirstOrDefault(e => e.Email == email);
            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            return Json(new { id = employee.Id, name = employee.Name });
        }

        [HttpGet("GetTeam/{teamId}")]
        public async Task<IActionResult> GetTeam(int teamId)
        {
            var members = await _context.Teams
                .Where(m => m.TeamId == teamId)
                .Select(m => new
                {
                    m.Id,
                    m.MemberName,
                    m.MemberEmail
                })
                .ToListAsync();

            return Ok(members);
        }

        [HttpGet]
        public IActionResult GetTeamMembers(int teamId)
        {
            var members = _context.Teams
                .Where(t => t.TeamId == teamId)
                .Select(m => new
                {
                    memberName = m.MemberName,
                    memberEmail = m.MemberEmail,
                    specialization = m.Specialization
                }).ToList();

            return Json(members);
        }

        [HttpGet]
        public IActionResult GetMemberDetails(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required");

            var member = _memberService.GetMemberByEmail(email);
            if (member != null)
            {
                return Json(new { memberName = member.Name, memberId = member.Id });
            }

            return Json(null);
        }

        [HttpPost]
        public async Task<IActionResult> AssignTask(string Email, string TaskTitle, string Technology, DateTime DueDate, string TaskDescription)
        {
            ViewBag.AdminName = User.Identity?.Name ?? "Admin";

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == Email);
            if (employee == null)
                return NotFound("Employee not found.");

            var assignedDate = DateTime.Now;

            var task = new EmployeeTask
            {
                EmployeeId = employee.Id,
                TaskTitle = TaskTitle,
                Technology = Technology,
                TaskDescription = TaskDescription,
                DueDate = DueDate,
                AssignedDate = assignedDate,
                Status = "Assigned",
                AssignedById = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Admin"
            };

            _context.EmployeeTasks.Add(task);

            var assignment = new TaskAssignment
            {
                Email = employee.Email,
                EmployeeId = employee.Id,
                TaskTitle = TaskTitle,
                TaskDescription = TaskDescription,
                Technology = Technology,
                AssignedDate = assignedDate,
                DueDate = DueDate,
                AssignedBy = ViewBag.AdminName
            };

            _context.TaskAssignments.Add(assignment);

            await _context.SaveChangesAsync();

            await _emailService.SendTaskAssignedEmail(
                employee.Email,
                employee.Name,
                TaskTitle,
                Technology,
                TaskDescription,
                assignedDate,
                DueDate
            );

            return RedirectToAction("AdminDashboard", "Dashboard");
        }

        [HttpGet]
        public IActionResult AssignTeamTask()
        {
            return View("AssignTeamTask");
        }

        [HttpPost]
        public async Task<IActionResult> AssignTeamTask(
            [FromForm] List<string> TeamEmails,
            [FromForm] List<string> MemberNames,
            [FromForm] List<string> MemberIds,
            [FromForm] List<string> TaskIds,
            [FromForm] string TaskTitle,
            [FromForm] string TaskTypes,
            [FromForm] string Technology,
            [FromForm] DateTime DueDate,
            [FromForm] string TaskDescription)
        {
            ViewBag.AdminName = User.Identity?.Name ?? "Admin";

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input data.";
                return View("AssignTeamTask");
            }

            if (TeamEmails == null || MemberNames == null || MemberIds == null || TaskIds == null)
            {
                ModelState.AddModelError("", "Input lists cannot be null.");
                return View("AssignTeamTask");
            }

            if (TeamEmails.Count != MemberNames.Count || TeamEmails.Count != MemberIds.Count || TeamEmails.Count != TaskIds.Count)
            {
                ModelState.AddModelError("", "Input counts do not match.");
                return View("AssignTeamTask");
            }

            var invalidEmails = new List<string>();

            for (int i = 0; i < TeamEmails.Count; i++)
            {
                var email = TeamEmails[i];
                var memberName = MemberNames[i];
                var memberId = MemberIds[i];
                var taskId = TaskIds[i];

                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
                if (employee == null)
                {
                    invalidEmails.Add(email);
                    continue;
                }

                var assignment = new TeamAssign
                {
                    Email = email,
                    MemberName = memberName,
                    MemberId = memberId,
                    TaskTitle = TaskTitle,
                    TaskTypes = TaskTypes,
                    Technology = Technology,
                    TaskDescription = TaskDescription,
                    DueDate = DueDate,
                    AssignedDate = DateTime.Now,
                    AssignedBy = ViewBag.AdminName
                };

                _context.TeamAssigns.Add(assignment);

                _taskService.AssignTaskToMember(email, memberId, memberName, taskId);

                await _emailService.SendTeamTaskAssignedEmailAsync(
                    email,
                    memberName,
                    TaskTitle,
                    Technology,
                    TaskDescription,
                    assignment.AssignedDate,
                    DueDate
                );
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Database save error: {ex.Message}";
                return View("AssignTeamTask");
            }

            if (invalidEmails.Any())
            {
                TempData["WarningMessage"] = $"Tasks assigned, but these emails were invalid: {string.Join(", ", invalidEmails)}";
            }
            else
            {
                TempData["SuccessMessage"] = "Tasks assigned to all team members successfully!";
            }

            return RedirectToAction("AdminDashboard", "Dashboard");
        }

        [HttpGet]
        public IActionResult Chat()
        {
            var employees = _context.Employees.ToList();
            return View("/Views/Admin/Chat.cshtml", employees);
        }

        [HttpGet]
        public async Task<IActionResult> ChatBox(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                return NotFound();

            return View("/Views/Admin/ChatBox.cshtml", employee);
        }

        [HttpGet]
        public async Task<IActionResult> AssignedTasks()
        {
            var tasks = await _context.EmployeeTasks
                .Include(t => t.AssignedBy)
                .ToListAsync();

            return View("/Views/Admin/AssignedTasks.cshtml", tasks);
        }

        [HttpGet]
        public async Task<IActionResult> ShowAssignedTasks()
        {
            string currentAdmin = User.Identity?.Name ?? "Admin";

            var assignedTasks = await _context.TaskAssignments
                .Where(t => t.AssignedBy == currentAdmin)
                .OrderByDescending(t => t.AssignedDate)
                .ToListAsync();

            return View("/Views/Admin/ShowAssignedTasks.cshtml", assignedTasks);
        }

        [HttpPost]
        public IActionResult DeleteTask(int id)
        {
            var task = _context.TaskAssignments.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                _context.TaskAssignments.Remove(task);
                _context.SaveChanges();
            }

            return RedirectToAction("ShowAssignedTasks", "Admin");
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeDashboard()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Users.FirstOrDefaultAsync(e => e.Email == email);

            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            ViewBag.EmployeeName = employee.Name;
            ViewBag.EmployeeEmail = employee.Email;

            ViewBag.TotalTasks = await _context.EmployeeTasks
                                        .Where(t => t.EmployeeId == employee.Id)
                                        .CountAsync();

            ViewBag.CompletedTasks = await _context.EmployeeTasks
                                        .Where(t => t.EmployeeId == employee.Id && t.Status == "Completed")
                                        .CountAsync();

            ViewBag.PendingTasks = await _context.EmployeeTasks
                                        .Where(t => t.EmployeeId == employee.Id && t.Status != "Completed")
                                        .CountAsync();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LeaveDashboard()
        {
            // Assuming you have a DbSet<LeaveRequest> in your DbContext
            var leaves = await _context.LeaveRequests.ToListAsync();
            return View(leaves);
        }
[HttpPost]
public async Task<IActionResult> Approve(int id)
{
    var leave = await _context.LeaveRequests.FindAsync(id);
    if (leave == null)
        return NotFound();

    leave.Status = "Approved";
    await _context.SaveChangesAsync();

    // Optional: send email notification using _emailService
    // await _emailService.SendLeaveStatusEmail(leave.EmployeeEmail, "Approved");

    return RedirectToAction("AdminDashboard", "Dashboard");

}

[HttpPost]
public async Task<IActionResult> Reject(int id)
{
    var leave = await _context.LeaveRequests.FindAsync(id);
    if (leave == null)
        return NotFound();

    leave.Status = "Rejected";
    await _context.SaveChangesAsync();

    // Optional: send email notification using _emailService
    // await _emailService.SendLeaveStatusEmail(leave.EmployeeEmail, "Rejected");

   return RedirectToAction("AdminDashboard", "Dashboard");

}



        
    }
}
