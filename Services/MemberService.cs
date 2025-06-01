using RoleBasedJWTMVC.Data;
using RoleBasedJWTMVC.Models;
using System.Linq;

namespace RoleBasedJWTMVC.Services
{
    public class MemberService
    {
        private readonly ApplicationDbContext _context;

        public MemberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Employee GetMemberByEmail(string email)
        {
            return _context.Employees.FirstOrDefault(e => e.Email == email);
        }
    }
}
