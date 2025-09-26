using SocialMediaApp.Core.DTO.AuthDTO;
using SocialMediaApp.Core.DTO.ResultDTO;

namespace SocialMediaApp.Core.Services
{
    public interface IAuthService
    {
        Task<AuthResultDTO> GoogleLoginAsync(GoogleAuthDTO request);
        Task<AuthResultDTO> LogIn(LogInDTO logInDTO);//
        Task<StringResult> Registe(RegisterDTO model);//
        Task<IntResult> ChangePasswordAsync(ChangePasswordDTO newPassword, string userId);//
        Task<AuthResultDTO> CheckRefreshTokenAndRevokeAndInvokeNewOne(string token, string userId);//
        Task<IntResult> ConfirmEmailAsync(VerifyCodeDTO request);//
        Task<IntResult> ForgetPasswordAsync(string email);//
        Task<AuthResultDTO> VerifyResetCodeAsync(VerifyCodeDTO request);//
        Task<IntResult> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO);//
        Task<IntResult> ResendConfirmationCodeAsync(string email);//
        Task<IntResult> ResendResetPasswordCodeAsync(string Email);//
        Task<IntResult> LogOutAsync(string userId, string refreshToken);        
    }
}
