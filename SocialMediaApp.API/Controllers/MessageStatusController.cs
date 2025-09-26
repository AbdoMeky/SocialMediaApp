using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.Interface;
using System.Security.Claims;

namespace SocialMediaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageStatusController : ControllerBase
    {
        private readonly IMessageStatusForChatMemberRepository _statusRepo;

        public MessageStatusController(IMessageStatusForChatMemberRepository statusRepo)
        {
            _statusRepo = statusRepo;
        }

        [HttpGet("message-status/{messageId}")]
        [Authorize]
        public async Task<ActionResult> GetStatusOfMessage(int messageId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _statusRepo.GetStatusOfMessage(userId, messageId);
            if (result == null)
                return NotFound("Message not found or you're not the sender.");

            return Ok(result);
        }

        /*[HttpPut("make-received/{messageId}")]
        [Authorize]
        public async Task<ActionResult> MarkAsReceived(int messageId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _statusRepo.MakeItReseaved(userId, messageId);
            if (result.Id == 0)
                return BadRequest(result.Message);

            return Ok(result);
        }*/

        [HttpPut("make-seen/{messageId}")]
        [Authorize]
        public async Task<ActionResult> MarkAsSeen(int messageId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _statusRepo.MakeItSeen(userId, messageId);
            if (result.Id == 0)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPut("make-all-received")]
        [Authorize]
        public async Task<ActionResult> MarkAllAsReceived()
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _statusRepo.MakeItReseavedForOnlineUser(userId);
            if (result.Id == 0)
                return BadRequest(result.Message);

            return Ok(result);
        }

        /*[HttpPut("make-chat-seen/{chatId}")]
        [Authorize]
        public async Task<ActionResult> MarkChatAsSeen(int chatId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _statusRepo.MakeItSeenForUserChat(userId, chatId);
            if (result.Id == 0)
                return BadRequest(result.Message);

            return Ok(result);
        }*/
    }
}
