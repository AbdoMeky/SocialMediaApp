using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.DTO.AuthDTO;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.Services;
using SocialMediaApp.Services.Extensions;
using System.Security.Claims;

namespace SocialMediaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Register([FromForm] RegisterDTO model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState.ExtractErrors());
            }
            var result = await _authService.Registe(model);
            if (string.IsNullOrEmpty(result.Message))
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] LogInDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ExtractErrors());
            }
            var result = await _authService.LogIn(loginDto);
            if (string.IsNullOrEmpty(result.Message))
            {
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    SetRefreshTokenInCookie(result.RefreshToken, (DateTime)result.RefreshTokenExpired);
                }
                return Ok(result);
            }
            return BadRequest(result.Message);
        }
        private void SetRefreshTokenInCookie(string refreshToken, DateTime expiry)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expiry,
                Secure = true,
                SameSite = SameSiteMode.Strict //lax
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<ActionResult> GoogleLogin([FromBody] GoogleAuthDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ExtractErrors());
            }
            var result = await _authService.GoogleLoginAsync(request);
            if (string.IsNullOrEmpty(result.Message))
            {
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    SetRefreshTokenInCookie(result.RefreshToken, (DateTime)result.RefreshTokenExpired);
                }
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ExtractErrors());
            }
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not logged in.");

            var result = await _authService.ChangePasswordAsync(dto, userId);
            if (string.IsNullOrEmpty(result.Message))
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("confirm-email")]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail([FromBody] VerifyCodeDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ExtractErrors());
            }
            var result = await _authService.ConfirmEmailAsync(dto);
            if (string.IsNullOrEmpty(result.Message))
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("forget-password")]
        [AllowAnonymous]
        public async Task<ActionResult> ForgetPassword([FromQuery] string email)
        {
            var result = await _authService.ForgetPasswordAsync(email);
            if (string.IsNullOrEmpty(result.Message))
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("verify-reset-code")]
        [AllowAnonymous]
        public async Task<ActionResult> VerifyResetCode([FromBody] VerifyCodeDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ExtractErrors());
            }
            var result = await _authService.VerifyResetCodeAsync(dto);
            if (string.IsNullOrEmpty(result.Message))
            {
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    SetRefreshTokenInCookie(result.RefreshToken, (DateTime)result.RefreshTokenExpired);
                }
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("reset-password")]
        [Authorize]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            var purposeClaim = User?.FindFirst("Purpose")?.Value;

            if (purposeClaim != "ResetPassword")
            {
                return Unauthorized("This token is not valid for resetting password.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ExtractErrors());
            }
            var result = await _authService.ResetPasswordAsync(dto);
            if (string.IsNullOrEmpty(result.Message))
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("resend-confirmation-code")]
        [AllowAnonymous]
        public async Task<ActionResult> ResendConfirmationCode([FromQuery] string email)
        {
            var result = await _authService.ResendConfirmationCodeAsync(email);
            return Ok(result);
        }

        [HttpPost("resend-reset-password-code")]
        [AllowAnonymous]
        public async Task<ActionResult> ResendResetPasswordCode([FromQuery] string email)
        {
            var result = await _authService.ResendResetPasswordCodeAsync(email);
            if (string.IsNullOrEmpty(result.Message))
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("refresh-token-revoke-and-invoke")]
        [Authorize]
        public async Task<ActionResult> RefreshToken()
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not logged in.");
            var refreshToken = Request.Cookies["refreshToken"];
            var result = await _authService.CheckRefreshTokenAndRevokeAndInvokeNewOne(refreshToken, userId);
            if (string.IsNullOrEmpty(result.Message))
            {
                if (!string.IsNullOrEmpty(result.RefreshToken))
                {
                    SetRefreshTokenInCookie(result.RefreshToken, (DateTime)result.RefreshTokenExpired);
                }
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not logged in.");
            var refreshToken = Request.Cookies["refreshToken"];
            var result = await _authService.LogOutAsync(userId, refreshToken);
            Response.Cookies.Delete("refreshToken");
            return Ok(result);
        }
    }
}
