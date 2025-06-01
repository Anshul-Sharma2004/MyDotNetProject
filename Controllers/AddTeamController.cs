using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoleBasedJWTMVC.Data;
using RoleBasedJWTMVC.Models;

namespace RoleBasedJWTMVC.Controllers
{
    public class TeamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeamController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View("/Views/Teams/Add.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Add(Team model)
        {
            if (!ModelState.IsValid)
            {
                return View("Views/Teams/Add.cshtml");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.MemberEmail);

            if (existingUser == null)
            {
                ModelState.AddModelError("MemberEmail", "This email is not registered.");
                return View("Views/Teams/Add.cshtml");
            }

            _context.Teams.Add(new Team
            {
                TeamId = model.TeamId,
                MemberName = model.MemberName,
                MemberEmail = model.MemberEmail,
                Specialization = model.Specialization
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Team member added successfully.";

            return RedirectToAction("List");
        }

        [HttpGet]
       
        public async Task<IActionResult> List()
        {
            var teams = await _context.Teams.ToListAsync();
            return View("/Views/Teams/List.cshtml", teams);
        }

    }
}
