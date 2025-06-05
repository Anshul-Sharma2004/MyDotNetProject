using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using RoleBasedJWTMVC.Data;
using RoleBasedJWTMVC.DTOs;
using RoleBasedJWTMVC.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace RoleBasedJWTMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View(dto);
            }

            var user = new User
            {
                RegisterID = dto.RegisterID,
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                Department = dto.Department,
                PhoneNumber = dto.PhoneNumber
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var profile = new Profile
            {
                Email = dto.Email,
                Name = dto.Name,
                Role = dto.Role,
                Department = dto.Department,
                PhoneNumber = dto.PhoneNumber
            };

            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();

            if (dto.Role == "Admin")
            {
                var existingAdmin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == dto.Email);
                if (existingAdmin == null)
                {
                    _context.Admins.Add(new Admin { Name = dto.Name, Email = dto.Email });
                }
                else
                {
                    existingAdmin.Name = dto.Name;
                    _context.Admins.Update(existingAdmin);
                }
                await _context.SaveChangesAsync();
            }
            else if (dto.Role == "Employee")
            {
                var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == dto.Email);
                if (existingEmployee == null)
                {
                    _context.Employees.Add(new Employee { Name = dto.Name, Email = dto.Email });
                }
                else
                {
                    existingEmployee.Name = dto.Name;
                    _context.Employees.Update(existingEmployee);
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

       [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginDto dto)
{
    if (!ModelState.IsValid)
        return View(dto);

    var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
    if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
    {
        ModelState.AddModelError("LoginFailed", "Invalid credentials.");
        return View(dto);
    }

    // Claims for Cookie Authentication - added ClaimTypes.Email
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Name), // So User.Identity.Name = user.Name
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email), // Added email claim here
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("RegisterID", user.RegisterID.ToString()),
        new Claim("Name", user.Name ?? ""),
        new Claim("PhoneNumber", user.PhoneNumber ?? ""),
        new Claim("Department", user.Department ?? "")
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
    });

    // Generate JWT token as before
    var token = GenerateJwtToken(user);
    Response.Cookies.Append("jwt", token, new CookieOptions
    {
        HttpOnly = true,
        Secure = Request.IsHttps,
        Expires = DateTimeOffset.UtcNow.AddDays(1)
    });

    return user.Role switch
    {
        "Admin" => RedirectToAction("AdminDashboard", "Dashboard"),
        "Employee" => RedirectToAction("EmployeeDashboard", "Dashboard"),
        _ => RedirectToAction("Login")
    };
}

private string GenerateJwtToken(User user)
{
    var jwtKey = _config["Jwt:Key"];
    if (string.IsNullOrEmpty(jwtKey))
        throw new Exception("JWT Key is not configured in appsettings.json");

    var key = Encoding.UTF8.GetBytes(jwtKey);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("RegisterID", user.RegisterID.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("Department", user.Department ?? "")
        }),
        Expires = DateTime.UtcNow.AddDays(1),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature)
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}


      [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Logout()
{
    Response.Cookies.Delete("jwt");
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return RedirectToAction("Login", "Auth");
}

    }
}
