using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.DTO.User;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Services.Extensions;
using System.Security.Claims;

namespace SocialMediaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentOnlineUserRepository _currentOnlineUserRepository;

        public UserController(IUserRepository userRepository, ICurrentOnlineUserRepository currentOnlineUserRepository)
        {
            _userRepository = userRepository;
            _currentOnlineUserRepository = currentOnlineUserRepository;
        }

        [HttpGet("get-user")]
        [Authorize]
        public async Task<IActionResult> GetUser()
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _userRepository.GetUser(userId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut("change-name")]
        [Authorize]
        public async Task<IActionResult> ChangeName([FromBody] ChangeNameDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ExtractErrors());

            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _userRepository.ChangeName(userId, dto.FirstName, dto.LastName);
            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPut("change-profile-picture")]
        [Authorize]
        public async Task<IActionResult> ChangeProfilePicture([FromForm] UpdateProfilePictureUrlDTO image)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _userRepository.ChangeProfilePictureUrl(image, userId);
            if (!string.IsNullOrEmpty(result.Message))
                return BadRequest(result.Message);

            return Ok(result);
        }
        [HttpGet("search/usernameORNumberOREmail")]
        [Authorize]
        public async Task<IActionResult> Search([FromQuery] string searchKey)
        {
            if (string.IsNullOrEmpty(searchKey))
                return BadRequest("Search key is required");

            var result = await _userRepository.Search(searchKey);
            return Ok(result);
        }
        [HttpGet("IsOnline/{userId}")]
        [Authorize]
        public async Task<IActionResult> IsOnline(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");
            var isOnline = await _currentOnlineUserRepository.IsOnline(userId);
            return Ok(new { UserId = userId, IsOnline = isOnline });
        }
    }
}
