using MimeKit;
using MimeKit.Text;

namespace Services.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(MimeMessage message);
        Task SendMessageAsync(string recipientName, string recipientMailAddress, TextFormat format, string subject, string content);
    }
}
