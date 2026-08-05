using Microsoft.Extensions.Configuration;
using RecyclingApp.Application.Common.Interfaces;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace RecyclingApp.Infrastructure.Email;

/// <summary>
/// Infrastructure implementation of IEmailSender using System.Net.Mail SMTP Client.
/// </summary>
public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var host = _configuration["Smtp:Host"] ?? "localhost";
        var port = int.Parse(_configuration["Smtp:Port"] ?? "25");
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        var enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"] ?? "false");
        var fromAddress = _configuration["Smtp:FromAddress"] ?? "no-reply@recyclingapp.com";
        var fromName = _configuration["Smtp:FromName"] ?? "Recycling App Support";

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Credentials = string.IsNullOrWhiteSpace(username) 
                ? null 
                : new NetworkCredential(username, password)
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(to);

        try
        {
            await client.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            // In a real application, you'd log this error.
            // Throwing it ensures the handler knows email delivery failed.
            throw new Exception($"Failed to send email to {to} via SMTP: {ex.Message}", ex);
        }
    }
}
