using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RoleBasedJWTMVC.Models; 

namespace RoleBasedJWTMVC.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendTaskAssignedEmail(string toEmail, string employeeName, string taskTitle, string technology, string description, DateTime assignDate, DateTime dueDate)
        {
            var subject = $"New Task Assigned: {taskTitle}";
            var body = $@"
                <p>Dear {employeeName},</p>
                <p>You have been assigned a new task.</p>
                <p><strong>Title:</strong> {taskTitle}</p>
                <p><strong>Description:</strong> {description}</p>
                <p><strong>Technology:</strong> {technology}</p>
                <p><strong>Assigned Date:</strong> {assignDate:dd-MM-yyyy}</p>
                <p><strong>Due Date:</strong> {dueDate:dd-MM-yyyy}</p>
                <br />
                <p>Kindly check your dashboard to begin working.</p>
                <p>Regards,<br/>Admin Team</p>";

            try
            {
                await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error for {toEmail}: {ex.Message}");
            }
        }

        public async Task SendTeamTaskAssignedEmailsAsync(List<TeamAssign> teamAssignments)
        {
            foreach (var assignment in teamAssignments)
            {
                var subject = $"New Team Task Assigned: {assignment.TaskTitle}";
                var body = $@"
                    <p>Dear {assignment.MemberName},</p>
                    <p>You have been assigned a new team task.</p>
                    <p><strong>Title:</strong> {assignment.TaskTitle}</p>
                    <p><strong>Description:</strong> {assignment.TaskDescription}</p>
                    <p><strong>Technology:</strong> {assignment.Technology}</p>
                    <p><strong>Assigned Date:</strong> {assignment.AssignedDate:dd-MM-yyyy}</p>
                    <p><strong>Due Date:</strong> {assignment.DueDate:dd-MM-yyyy}</p>
                    <br />
                    <p>Kindly coordinate with your team and start working.</p>
                    <p>Regards,<br/>Admin Team</p>";

                try
                {
                    await SendEmailAsync(assignment.Email, subject, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email to {assignment.Email}: {ex.Message}");
                }
            }
        }

        public async Task SendTeamTaskAssignedEmailAsync(string toEmail, string memberName, string taskTitle, string technology, string taskDescription, DateTime assignedDate, DateTime dueDate)
        {
            var subject = "New Team Task Assigned";
            var body = $@"
                <p>Hello {memberName},</p>
                <p>You have been assigned a new team task:</p>
                <p><strong>Title:</strong> {taskTitle}</p>
                <p><strong>Technology:</strong> {technology}</p>
                <p><strong>Description:</strong> {taskDescription}</p>
                <p><strong>Assigned Date:</strong> {assignedDate:yyyy-MM-dd}</p>
                <p><strong>Due Date:</strong> {dueDate:yyyy-MM-dd}</p>
                <br />
                <p>Please check the system for more details.</p>";

            try
            {
                await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error for {toEmail}: {ex.Message}");
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = _config["EmailSettings:FromEmail"];
            var password = _config["EmailSettings:Password"];
            var host = _config["EmailSettings:SmtpHost"];
            var port = int.Parse(_config["EmailSettings:Port"]);

            using var smtpClient = new SmtpClient(host)
            {
                Port = port,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, "Admin | Task Scheduler"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}