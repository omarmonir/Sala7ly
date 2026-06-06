using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Sala7ly.BLL.Services.Abstraction;

namespace Sala7ly.BLL.Services.Implementation
{
    public class EmailService(IConfiguration configuration) : IEmailService
    {
        private readonly IConfiguration _configuration = configuration;

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var smtpClient = new SmtpClient(_configuration["EmailSettings:SMTPHost"])
                {
                    Port = int.Parse(_configuration["EmailSettings:SMTPPort"]!),
                    Credentials = new NetworkCredential(
                        _configuration["EmailSettings:SenderEmail"],
                        _configuration["EmailSettings:SenderPassword"]
                    ),
                    EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSSL"]!)
                };

                using var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_configuration["EmailSettings:SenderEmail"]!);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                Console.WriteLine($"[DEBUG] toEmail raw: '{toEmail}'");
                Console.WriteLine($"[DEBUG] toEmail trimmed: '{toEmail.Trim()}'");
                Console.WriteLine($"[DEBUG] toEmail bytes: {string.Join(" ", System.Text.Encoding.UTF8.GetBytes(toEmail.Trim()).Select(b => b.ToString("X2")))}");
                mailMessage.To.Add(new MailAddress(toEmail.Trim()));


                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error: {ex}");
                return false;
            }
        }
    }
}