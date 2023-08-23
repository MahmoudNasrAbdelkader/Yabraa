using EmailService;

namespace Yabraa.IServices
{
    public interface IEmailSender
    {
        void SendEmail(Message message, bool EnableHtml = false);
        Task SendEmailAsync(Message message, bool EnableHtml = false);

    }
}
