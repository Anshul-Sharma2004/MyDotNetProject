using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoleBasedJWTMVC.Data;
using RoleBasedJWTMVC.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;


namespace RoleBasedJWTMVC.Controllers
{
    [Authorize]
    public class LeaveController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public LeaveController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Employee - Submit Leave Request (GET)
        [HttpGet]
        public IActionResult RequestLeave() => View("/Views/DashboardData/RequestLeave.cshtml");

        // Employee - Submit Leave Request (POST)
        [HttpPost]
        public async Task<IActionResult>  RequestLeave(LeaveRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Status = LeaveStatus.Pending;
            model.RequestDate = DateTime.Now;

            _context.LeaveRequests.Add(model);
            await _context.SaveChangesAsync();

            SendEmailToAdmin(model);

            TempData["SuccessMessage"] = "Leave request submitted successfully!";
            return RedirectToAction("LeaveSuccess");
        }

        public IActionResult LeaveSuccess() => View("/Views/DashboardData/LeaveSuccess.cshtml");

        // Employee - View Their Leave Requests
       [HttpGet]
    public async Task<IActionResult> EmployeeLeaveStatus()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized("Email not found in user claims.");
        }

        var leaves = await _context.LeaveRequests
            .Where(l => l.EmployeeEmail == email)
            .OrderByDescending(l => l.RequestDate)
            .ToListAsync();

        return View("/Views/Leave/EmployeeLeaveStatus.cshtml", leaves);
    }


        // Admin - View All Leave Requests
        // [Authorize(Roles = "Admin")]
        // [HttpGet]
        // public async Task<IActionResult> AdminLeaveRequests()
        // {
        //     var leaves = await _context.LeaveRequests
        //         .OrderByDescending(l => l.RequestDate)
        //         .ToListAsync();

        //     return View("/Views/Leave/AdminLeaveRequest.cshtml", leaves);
        // }

        // // Admin - Approve Leave Request
        // [Authorize(Roles = "Admin")]
        // [HttpPost]
        // public async Task<IActionResult> ApproveLeave(int id)
        // {
        //     var leave = await _context.LeaveRequests.FindAsync(id);
        //     if (leave == null)
        //         return NotFound();

        //     leave.Status = LeaveStatus.Accepted;
        //     await _context.SaveChangesAsync();

        //     SendEmailToEmployee(leave);

        //     TempData["SuccessMessage"] = $"Leave request for {leave.EmployeeName} approved.";
        //     return RedirectToAction("AdminLeaveRequests", "Leave");

        // }

        // // Admin - Reject Leave Request
        // [Authorize(Roles = "Admin")]
        // [HttpPost]
        // public async Task<IActionResult> RejectLeave(int id)
        // {
        //     var leave = await _context.LeaveRequests.FindAsync(id);
        //     if (leave == null)
        //         return NotFound();

        //     leave.Status = LeaveStatus.Rejected;
        //     await _context.SaveChangesAsync();

        //     SendEmailToEmployee(leave);

        //     TempData["SuccessMessage"] = $"Leave request for {leave.EmployeeName} rejected.";
        //     return RedirectToAction("AdminLeaveRequests", "Leave");

        // }

        // Email Helpers
        private void SendEmailToAdmin(LeaveRequest request)
        {
            var adminEmail = _configuration["EmailSettings:AdminEmail"];
            var fromEmail = _configuration["EmailSettings:FromEmail"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(fromEmail))
                return;

            var message = new MailMessage(fromEmail, adminEmail)
            {
                Subject = "New Leave Request",
                Body = $"Leave request from {request.EmployeeName}\n" +
                       $"Email: {request.EmployeeEmail}\n" +
                       $"Reason: {request.Reason}\n" +
                       $"From: {request.StartDate:d} To: {request.EndDate:d}"
            };

            SendMail(message);
        }

        private void SendEmailToEmployee(LeaveRequest request)
        {
            var fromEmail = _configuration["EmailSettings:FromEmail"];

            if (string.IsNullOrWhiteSpace(request.EmployeeEmail) || string.IsNullOrWhiteSpace(fromEmail))
                return;

            var message = new MailMessage(fromEmail, request.EmployeeEmail)
            {
                Subject = $"Leave Request {request.Status}",
                Body = $"Hello {request.EmployeeName}, your leave from {request.StartDate:d} to {request.EndDate:d} has been {request.Status}."
            };

            SendMail(message);
        }

        private void SendMail(MailMessage message)
        {
            var host = _configuration["EmailSettings:SmtpHost"];
            var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var password = _configuration["EmailSettings:Password"];

            using var smtp = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            smtp.Send(message);
        }
        
        
    }
}
