using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.DTO.MessageDTO;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Core.Interface.Factory;
using System.Security.Claims;

namespace SocialMediaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupChatMessageController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;

        public GroupChatMessageController(IMessageRepositoryFactory factory)
        {
            _messageRepository = factory.CreateRepository("GROUP");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddMessage( AddMessageDTO Message)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("You must be logged in.");

            var result = await _messageRepository.Add(userId, Message);
            if (result.Id == 0)
                return BadRequest(result.Message);

            var url = Url.Action(nameof(GetMessage), new { Id = result.Id });
            return Created(url,await _messageRepository.GetWithOutUserId(result.Id));
        }

        [HttpDelete("{Id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteMessage(int Id)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("You must be logged in.");

            var result = await _messageRepository.Delete(userId, Id);
            if (result.Id == 0)
                return BadRequest(result.Message);
            return NoContent();
        }

        [HttpGet("{Id:int}")]
        [Authorize]
        public async Task<IActionResult> GetMessage(int Id)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("You must be logged in.");

            var result = await _messageRepository.Get(userId, Id);
            if (result == null)
                return NotFound("Message not found or you're not allowed to see it.");

            return Ok(result);
        }

        [HttpGet("chat/{ChatId:int}/{Page:int}")]
        [Authorize]
        public async Task<IActionResult> GetMessagesInChat(int ChatId,int Page)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("You must be logged in.");

            var messages = await _messageRepository.ShowMessageInChat(userId, ChatId,Page);
            if (messages == null)
                return NotFound("You are not a member in this chat.");

            return Ok(messages);
        }
    }
}
