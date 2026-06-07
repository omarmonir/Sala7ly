namespace Sala7ly.BLL.Services.Abstraction
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }
}
