using System.IO;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _templatePath;

    public EmailService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _templatePath = Path.Combine(
            environment.ContentRootPath,
            "API",
            "Templates",
            "EmailConfirmation.html"
        );
    }

    private async Task<string> GetEmailTemplate(string confirmationUrl)
    {
        string template = await File.ReadAllTextAsync(_templatePath);
        return template.Replace("{confirmationUrl}", confirmationUrl);
    }

    public async Task SendConfirmationEmail(string email, string confirmationToken)
    {
        try
        {
            var confirmationUrl =
                $"https://{Environment.GetEnvironmentVariable("APPLICATION_BASE_URL") ?? _configuration["Application:BaseUrl"]}/api/users/confirm-email?token={confirmationToken}&email={email}";
            var emailBody = await GetEmailTemplate(confirmationUrl);

            var smtpClient = new SmtpClient
            {
                Host =
                    Environment.GetEnvironmentVariable("GMAIL_SMTP_SERVER")
                    ?? _configuration["Gmail:SmtpServer"],
                Port = int.Parse(
                    Environment.GetEnvironmentVariable("GMAIL_SMTP_PORT")
                        ?? _configuration["Gmail:Port"]
                ),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    Environment.GetEnvironmentVariable("GMAIL_USERNAME")
                        ?? _configuration["Gmail:Username"],
                    Environment.GetEnvironmentVariable("GMAIL_PASSWORD")
                        ?? _configuration["Gmail:Password"]
                )
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(
                    Environment.GetEnvironmentVariable("GMAIL_FROM") ?? _configuration["Gmail:From"]
                ),
                Subject =
                    Environment.GetEnvironmentVariable("EMAIL_SUBJECT") ?? "Bekræft din email",
                Body = emailBody,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
            throw;
        }
    }
}
