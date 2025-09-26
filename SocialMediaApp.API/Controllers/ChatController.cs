using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.Interface;
using System.Security.Claims;

namespace SocialMediaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatRepository _chatRepository;
        public ChatController(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }
        [Authorize]
        [HttpGet("{page}/{size}")]
        public async Task<IActionResult> GetAsync(int page, int size)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("You must be logged in.");

            var result = await _chatRepository.GetChatsForUser(userId, page, size);
            if (result == null)
                return NotFound("Message not found or you're not allowed to see it.");

            return Ok(result);
        }

    }
}
