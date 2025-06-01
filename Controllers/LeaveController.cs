using Microsoft.AspNetCore.Mvc;
using RoleBasedJWTMVC.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using RoleBasedJWTMVC.Data;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace RoleBasedJWTMVC.Controllers
{
    public class LeaveController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public LeaveController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Employee - Request Leave (GET)
        [HttpGet]
        public IActionResult RequestLeave() => View("/Views/DashboardData/RequestLeave.cshtml");

        // Employee - Request Leave (POST)
        [HttpPost]
        public async Task<IActionResult> RequestLeave(LeaveRequest model)
        {
            if (ModelState.IsValid)
            {
                model.Status = "Pending";
                model.RequestDate = DateTime.Now;

                _context.LeaveRequests.Add(model);
                await _context.SaveChangesAsync();

                SendEmailToAdmin(model);
                return RedirectToAction("LeaveSuccess");
            }

            return View(model);
        }

        public IActionResult LeaveSuccess() => View("/Views/DashboardData/LeaveSuccess.cshtml");

        // Admin - View All Leave Requests
        public async Task<IActionResult> AdminDashboard()
        {
            var leaves = await _context.LeaveRequests
                .OrderByDescending(l => l.RequestDate)
                .ToListAsync();

            return View(leaves);
        }

        // Admin - Approve Leave
      [HttpPost]
public async Task<IActionResult> Approve(int id)
{
    var leave = await _context.LeaveRequests.FindAsync(id);
    if (leave != null)
    {
        leave.Status = "Approved";
        await _context.SaveChangesAsync();
        SendEmailToEmployee(leave);
    }
    return RedirectToAction("AdminDashboard");
}

[HttpPost]
public async Task<IActionResult> Reject(int id)
{
    var leave = await _context.LeaveRequests.FindAsync(id);
    if (leave != null)
    {
        leave.Status = "Rejected";
        await _context.SaveChangesAsync();
        SendEmailToEmployee(leave);
    }
    return RedirectToAction("AdminDashboard");
}


        // Email Helpers
        private void SendEmailToAdmin(LeaveRequest request)
        {
            var adminEmail = _configuration["EmailSettings:AdminEmail"];
            var fromEmail = _configuration["EmailSettings:FromEmail"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(fromEmail))
            {
                // Optional: Log or throw an exception that emails are not configured properly
                return;
            }

            var message = new MailMessage(fromEmail, adminEmail)
            {
                Subject = "New Leave Request",
                Body = $"Leave request from {request.EmployeeName}\n" +
                       $"Email: {request.EmployeeEmail}\n" +
                       $"Reason: {request.Reason}\n" +
                       $"From: {request.StartDate.ToShortDateString()} To: {request.EndDate.ToShortDateString()}"
            };

            SendMail(message);
        }

        private void SendEmailToEmployee(LeaveRequest request)
        {
            var fromEmail = _configuration["EmailSettings:FromEmail"];

            if (string.IsNullOrWhiteSpace(request.EmployeeEmail) || string.IsNullOrWhiteSpace(fromEmail))
            {
                // Optional: Log or throw an exception that emails are not configured properly
                return;
            }

            var message = new MailMessage(fromEmail, request.EmployeeEmail)
            {
                Subject = $"Leave Request {request.Status}",
                Body = $"Hello {request.EmployeeName}, your leave from {request.StartDate.ToShortDateString()} to {request.EndDate.ToShortDateString()} has been {request.Status}."
            };

            SendMail(message);
        }

        private void SendMail(MailMessage message)
        {
            var host = _configuration["EmailSettings:SmtpHost"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);
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

        // Employee - View Leave Status
[HttpGet]
public async Task<IActionResult> EmployeeLeaves(string email)
{
    if (string.IsNullOrEmpty(email))
    {
        return BadRequest("Email is required");
    }

    var leaves = await _context.LeaveRequests
        .Where(l => l.EmployeeEmail == email)
        .OrderByDescending(l => l.RequestDate)
        .ToListAsync();

    return View("/Views/DashboardData/EmployeeLeaves.cshtml", leaves);
}

    }
}
