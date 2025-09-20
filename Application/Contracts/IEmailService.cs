
using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string email, string confirmationToken);
        Task SendPasswordResetEmailAsync(string email, string resetToken);
        Task SendWelcomeEmailAsync(string email, string userName);
        Task SendPasswordChangedNotificationAsync(string email, string userName);
        Task SendOtpEmailAsync(string email, string otp, string purpose);
    }
}
