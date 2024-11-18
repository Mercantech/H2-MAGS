using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendConfirmationEmail(string email, string confirmationToken)
    {
        try
        {
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
                From = new MailAddress(_configuration["Gmail:From"]),
                Subject = "Bekræft din email",
                Body =
                    $@"
                    <h2>Velkommen til vores hotel-booking system!</h2>
                    <p>Klik på følgende link for at bekræfte din email:</p>
                    <a href='https://{_configuration["Application:BaseUrl"]}/api/users/confirm-email?token={confirmationToken}&email={email}'>
                        Bekræft min email
                    </a>",
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
