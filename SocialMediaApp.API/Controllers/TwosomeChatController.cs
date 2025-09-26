using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.Interface;
using System.Security.Claims;

namespace SocialMediaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TwosomeChatController : ControllerBase
    {
        private readonly ITwosomeChatRepository _twosomeChatRepository;

        public TwosomeChatController(ITwosomeChatRepository twosomeChatRepository)
        {
            _twosomeChatRepository = twosomeChatRepository;
        }

        [HttpPost("{userTwoId}")]
        [Authorize]
        public async Task<ActionResult> CreateChatWithUser(string userTwoId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _twosomeChatRepository.Add(userId, userTwoId);
            if (result.Id == 0)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("by-user-id/{userTwoId}")]
        [Authorize]
        public async Task<ActionResult> GetChatWithUser(string userTwoId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _twosomeChatRepository.Get(userId, userTwoId);
            if (result == null)
                return NotFound("Id not valid");

            return Ok(result);
        }

        /*[HttpGet("{chatId}")]
        [Authorize]
        public async Task<ActionResult> GetChatById(int chatId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _twosomeChatRepository.GetById(userId, chatId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }*/

        [HttpGet("by-chat-id/{chatId:int}")]
        [Authorize]
        public async Task<ActionResult> GetChatWithMessages(int chatId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _twosomeChatRepository.GetWithMessages(userId, chatId);
            if (result == null)
                return NotFound("Id not valid");

            return Ok(result);
        }
    }
}
