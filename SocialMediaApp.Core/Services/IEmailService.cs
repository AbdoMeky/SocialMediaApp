using SocialMediaApp.Core.DTO.ResultDTO;

namespace SocialMediaApp.Core.Services
{
    public interface IEmailService
    {
        Task<IntResult> SendEmailAsync(string email, string subject, string body);
        public string GenerateVerificatonCode(int length = 6);
    }
}
