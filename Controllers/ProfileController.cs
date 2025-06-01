using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleBasedJWTMVC.Data;
using RoleBasedJWTMVC.Models; 
using System.Linq;
using System.Security.Claims;

namespace RoleBasedJWTMVC.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Details()
        {
            // Get logged-in user's email from claims
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (email == null)
            {
                return Unauthorized();
            }

            // Fetch profile for this email
            var profile = _context.Profiles.FirstOrDefault(p => p.Email == email);

            if (profile == null)
            {
                // Optional: Handle missing profile case, e.g. redirect or show error
                return NotFound("Profile not found.");
            }

            // Pass profile model to the view
            return View(profile);
        }
    }
}
