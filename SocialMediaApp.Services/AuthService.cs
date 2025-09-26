using Google.Apis.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SocialMediaApp.Core.DTO.AuthDTO;
using SocialMediaApp.Core.DTO.ResultDTO;
using SocialMediaApp.Core.Entities;
using SocialMediaApp.Core.Helper;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Core.Services;
using SocialMediaApp.Core.Utilities;
using SocialMediaApp.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


namespace SocialMediaApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService emailService;
        private readonly IUserRepository repository;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly GoogleSettings _googleSettings;
        private readonly JWTSettings _jwtSettings;
        private readonly string _storagePath;
        private readonly string _backupDirPath;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _context;
        public AuthService(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService,
            IUserRepository repository,
            SignInManager<ApplicationUser> signInManager,
            GoogleSettings googleSettings,
            JWTSettings jwtSettings,
            IWebHostEnvironment webHostEnvironment,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            this.emailService = emailService;
            this.repository = repository;
            this.signInManager = signInManager;
            _googleSettings = googleSettings;
            _jwtSettings = jwtSettings;
            _webHostEnvironment = webHostEnvironment;
            _storagePath = Path.Combine(_webHostEnvironment.WebRootPath, "UploadedImagesForUser");
            _backupDirPath = Path.Combine(_webHostEnvironment.WebRootPath, "BackupForUserImage");
            _context = context;
        }
        //
        public async Task<StringResult> Registe(RegisterDTO model)
        {
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
            if (!Directory.Exists(_backupDirPath))
            {
                Directory.CreateDirectory(_backupDirPath);
            }
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
            {
                return new StringResult() { Message = "Email is used." };
            }
            if (await _userManager.FindByNameAsync(model.UserName) is not null)
            {
                return new StringResult() { Message = "Username is used." };
            }
            var filePath = AddImageHelper.chickImagePath(model.Image, _storagePath);
            if (!string.IsNullOrEmpty(filePath.Message))
            {
                return new StringResult() { Message = filePath.Message };
            }

            ApplicationUser user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                ProfilePictureUrl = filePath.Id
            };
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (!result.Succeeded)
                    {
                        return new StringResult
                        {
                            Message = string.Join(", ", result.Errors.Select(e => e.Description))
                        };
                    }
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (!roleResult.Succeeded)
                    {
                        return new StringResult
                        {
                            Message = string.Join(", ", roleResult.Errors.Select(e => e.Description))
                        };
                    }
                    if (model.Image != null)
                    {
                        await AddImageHelper.AddFile(model.Image, user.ProfilePictureUrl);
                    }
                    await SendConfirmationEmail(user);
                    await transaction.CommitAsync();
                    return new StringResult { Id = user.Id };
                }
                catch (Exception ex)
                {
                    AddImageHelper.DeleteFiles(filePath.Id);
                    return new StringResult() { Message = ex.Message };
                }
            }
        }
        //
        public async Task<IntResult> ChangePasswordAsync(ChangePasswordDTO newPassword, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new IntResult { Message = "you are not allow to do it." };
            }

            var oldresult = await _userManager.CheckPasswordAsync(user, newPassword.OldPassword);
            if (!oldresult)
            {
                return new IntResult { Message = "The old password is incorrect" };
            }
            var result = await _userManager.ChangePasswordAsync(user, newPassword.OldPassword, newPassword.NewPassword);

            if (result.Succeeded)
            {
                return new IntResult { Message = "Password changed successfully" };
            }
            return new IntResult
            {
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }
        //
        public async Task<AuthResultDTO> CheckRefreshTokenAndRevokeAndInvokeNewOne(string token, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthResultDTO() { Message = "not valid token" };
            }
            var refreshToken = user.RefreshTokens.FirstOrDefault(x => x.Token == token);
            if (refreshToken is null || !refreshToken.IsActive)
            {
                return new AuthResultDTO() { Message = "not active" };
            }
            refreshToken.RevokedOn = DateTime.UtcNow;
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            try
            {
                await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                return new AuthResultDTO { Message = ex.Message };
            }
            var newToken = await CreateToken(user);
            return new AuthResultDTO()
            {
                IsAuthenticated = true,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpired = newRefreshToken.ExpiredOn,
                Token = new JwtSecurityTokenHandler().WriteToken(newToken)
            };
        }
        //
        public async Task<IntResult> LogOutAsync(string userId, string refreshToken)
        {
            var user = await repository.GetWithRefreshToken(userId);
            if (user == null)
                throw new UnauthorizedAccessException();
            var token = user.RefreshTokens.FirstOrDefault(x => x.Token == refreshToken);
            if (token != null)
            {
                token.RevokedOn = DateTime.UtcNow;
            }
            await repository.SaveAsync();
            //await signInManager.SignOutAsync();
            return new IntResult { Id = 1 };
        }
        //
        public async Task<IntResult> ConfirmEmailAsync(VerifyCodeDTO request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new IntResult { Message = "User not found." };
            }

            var storedCode = await _userManager.GetAuthenticationTokenAsync(user,
                "EmailVerification", "VerificationCode");

            if (storedCode != request.VerificationCode)
            {
                return new IntResult { Message = "Invalid verification code." };
            }
            await _userManager.RemoveAuthenticationTokenAsync(user, "EmailVerification", "VerificationCode");

            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return new IntResult { Id = 1 };
            }
            return new IntResult { Message = "Email confirmation failed." };

        }
        //
        public async Task<IntResult> ForgetPasswordAsync(string email)
        {

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new IntResult { Message = "User not found." };
            }
            try
            {
                var verificationCode = emailService.GenerateVerificatonCode();

                await _userManager.SetAuthenticationTokenAsync(user, "ResetPassword",
                    "ResetPasswordCode", verificationCode);

                await emailService.SendEmailAsync(email, "Password Reset Verification Code",
                $"Your verification code is: <b>{verificationCode}</b>");

                return new IntResult { Id = 1 };
            }
            catch (Exception)
            {
                return new IntResult { Message = "An error occurred while sending the verification code. Please try again later." };
            }
        }
        //
        public async Task<AuthResultDTO> LogIn(LogInDTO logInDTO)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(logInDTO.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, logInDTO.Password))
            {
                return new AuthResultDTO { Message = "Email or Password is not correct" };
            }
            JwtSecurityToken JwtToken = await CreateToken(user);
            RefreshToken refreshToken;
            if (user.RefreshTokens.Any(t => t.IsActive))
            {
                refreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
            }
            else
            {
                refreshToken = GenerateRefreshToken();
                user.RefreshTokens.Add(refreshToken);
                try
                {
                    await _userManager.UpdateAsync(user);
                }
                catch (Exception ex)
                {
                    return new AuthResultDTO { Message = ex.Message };
                }
            }
            return new AuthResultDTO
            {
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(JwtToken),
                RefreshToken = refreshToken.Token,
                RefreshTokenExpired = refreshToken.ExpiredOn
            };
        }

        private async Task<JwtSecurityToken> CreateToken(ApplicationUser user, bool resetPassword = false)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,(user.FirstName+" "+user.LastName)??""),
                new Claim(JwtRegisteredClaimNames.Email,user.Email??""),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };
            if (resetPassword == true)
            {
                claims.Add(new Claim("Purpose", "ResetPassword"));
            }
            else
            {
                var Roles = await _userManager.GetRolesAsync(user);
                foreach (var Role in Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, Role));
                }
            }
            SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecritKey));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken myToken = new JwtSecurityToken(
                issuer: _jwtSettings.IssuerIP,
                audience: _jwtSettings.AudienceIP,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(120),
                signingCredentials: credentials);
            return myToken;
        }

        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNumber);
            return new RefreshToken()
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiredOn = DateTime.UtcNow.AddDays(1),
                CreatedOn = DateTime.UtcNow
            };
        }
        //
        public async Task<IntResult> ResendConfirmationCodeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new IntResult { Message = "User not found." };
            }
            if (user.EmailConfirmed)
            {
                return new IntResult { Message = "Email already confirmed." };
            }
            var verificationCode = emailService.GenerateVerificatonCode();
            await _userManager.SetAuthenticationTokenAsync(user, "EmailVerification", "VerificationCode", verificationCode);
            var emailBody = $"Your verification code is: <b>{verificationCode}</b>";
            await emailService.SendEmailAsync(user.Email, "Resend Verification Code", emailBody);

            return new IntResult { Id = 1 };

        }
        //
        public async Task<IntResult> ResendResetPasswordCodeAsync(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                return new IntResult { Message = "User not found." };
            }
            var verificationCode = emailService.GenerateVerificatonCode();
            await _userManager.SetAuthenticationTokenAsync(user, "ResetPassword", "ResetPasswordCode", verificationCode);

            var emailBody = $"Your password reset verification code is: <b>{verificationCode}</b>";
            await emailService.SendEmailAsync(user.Email, "Password Reset Verification Code", emailBody);

            return new IntResult { Id = 1 };
        }
        //
        public async Task<IntResult> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
        {
            var userModel = await _userManager.FindByEmailAsync(resetPasswordDTO.Email);
            if (userModel == null)
            {
                return new IntResult { Message = "User not found." };
            }

            var removePasswordResult = await _userManager.RemovePasswordAsync(userModel);

            if (!removePasswordResult.Succeeded)
            {
                return new IntResult { Message = "Failed to reset password." };
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(userModel, resetPasswordDTO.NewPassword);

            if (addPasswordResult.Succeeded)
            {
                return new IntResult { Id = 1 };
            }
            return new IntResult
            {
                Message = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description))
            };
        }

        public async Task<AuthResultDTO> VerifyResetCodeAsync(VerifyCodeDTO request)
        {
            var userModel = await _userManager.FindByEmailAsync(request.Email);
            if (userModel == null)
            {
                return new AuthResultDTO { Message = "User not found." };
            }

            var savedCode = await _userManager.GetAuthenticationTokenAsync(userModel,
                    "ResetPassword", "ResetPasswordCode");

            if (savedCode == request.VerificationCode)
            {
                await _userManager.RemoveAuthenticationTokenAsync(userModel, "ResetPassword", "ResetPasswordCode");

                var token = await CreateToken(userModel, true);
                var refreshToken = GenerateRefreshToken();
                userModel.RefreshTokens.Add(refreshToken);
                return new AuthResultDTO
                {
                    IsAuthenticated = true,
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken.Token,
                    RefreshTokenExpired = refreshToken.ExpiredOn
                };
            }
            return new AuthResultDTO { Message = "Invalid verification code." };
        }

        private async Task<IntResult> SendConfirmationEmail(ApplicationUser user)
        {

            if (string.IsNullOrEmpty(user.Email))
            {
                return new IntResult { Message = "User email is not set." };
            }

            var verificationCode = emailService.GenerateVerificatonCode();
            user.EmailConfirmed = false;

            await _userManager.SetAuthenticationTokenAsync(user, "EmailVerification", "VerificationCode", verificationCode);

            var emailBody = $"Your verification code is: <b>{verificationCode}</b>";

            await emailService.SendEmailAsync(user.Email, "Verify your email", emailBody);

            return new IntResult { Id = 1 };
        }

        public async Task<AuthResultDTO> GoogleLoginAsync(GoogleAuthDTO request)
        {
            try
            {
                var userInfo = await GetGoogleUserInfo(request.AccessToken);
                if (userInfo == null)
                    return new AuthResultDTO { Message = "Invalid Access Token or missing permissions." };

                var payload = await VerifyGoogleIdToken(request.IdToken);
                if (payload == null)
                    return new AuthResultDTO { Message = "Invalid Google token" };


                var user = await _userManager.FindByEmailAsync(userInfo.Email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = userInfo.Email,
                        Email = userInfo.Email,
                        FirstName = userInfo.FirstName,
                        LastName = userInfo.LastName,
                        ProfilePictureUrl = userInfo.Picture,
                        DateOfBirth = userInfo.Birthdate,
                    };
                    user.EmailConfirmed = true;
                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        return new AuthResultDTO
                        {
                            Message = string.Join(", ", result.Errors.Select(e => e.Description))
                        };
                    }

                    var Jwttoken = await CreateToken(user);
                    var refreshtoken = GenerateRefreshToken();
                    user.RefreshTokens.Add(refreshtoken);

                    await repository.SaveAsync();
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    return new AuthResultDTO
                    {
                        IsAuthenticated = true,
                        Token = new JwtSecurityTokenHandler().WriteToken(Jwttoken),
                        RefreshToken = refreshtoken.Token,
                        RefreshTokenExpired = refreshtoken.ExpiredOn
                    };
                }

                var roles = await _userManager.GetRolesAsync(user);


                var JwtToken = await CreateToken(user);
                var refreshToken = GenerateRefreshToken();
                user.RefreshTokens.Add(refreshToken);

                await repository.SaveAsync();
                return new AuthResultDTO
                {
                    IsAuthenticated = true,
                    Token = new JwtSecurityTokenHandler().WriteToken(JwtToken),
                    RefreshToken = refreshToken.Token,
                    RefreshTokenExpired = refreshToken.ExpiredOn
                };
            }
            catch (Exception ex)
            {
                return new AuthResultDTO
                {
                    Message = ex.Message
                };
            }
        }
        public async Task<GoogleJsonWebSignature.Payload> VerifyGoogleIdToken(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { _googleSettings.ClientID }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payload;
            }
            catch
            {
                return null;
            }
        }

        public async Task<GoogleUserInfoDTO> GetGoogleUserInfo(string accessToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("https://people.googleapis.com/v1/people/me?personFields=names,birthdays,emailAddresses,photos");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var googleData = JsonSerializer.Deserialize<GooglePeopleApiResponseDTO>(
                 content,
                 new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (googleData == null)
                return null;

            var fullName = googleData.Names?.FirstOrDefault()?.DisplayName?.Split(" ") ?? Array.Empty<string>();
            string firstName = fullName?.Length > 0 ? fullName[0] : "";
            string lastName = fullName?.Length > 1 ? string.Join(" ", fullName.Skip(1)) : "";

            var birthDate = googleData.Birthdays?.FirstOrDefault()?.Date;
            DateTime? birthdate = (birthDate != null) ? new DateTime(birthDate.Year, birthDate.Month, birthDate.Day) : null;

            return new GoogleUserInfoDTO
            {
                Email = googleData.EmailAddresses?.FirstOrDefault()?.Value ?? string.Empty,
                FirstName = firstName,
                LastName = lastName,
                Picture = googleData.Photos?.FirstOrDefault()?.Url ?? string.Empty,
                Birthdate = birthdate
            };
        }
    }
}
