using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.DTO.GroupChatDTO;
using SocialMediaApp.Core.DTO.User;
using SocialMediaApp.Core.Interface;
using System.Security.Claims;

namespace SocialMediaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupChatController : ControllerBase
    {
        private readonly IGroupChatRepository _groupChatRepository;

        public GroupChatController(IGroupChatRepository groupChatRepository)
        {
            _groupChatRepository = groupChatRepository;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromForm] AddGroupChatDTO dto)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _groupChatRepository.Add(userId, dto);
            if (result.Id == 0) return BadRequest(result.Message);
            string url = Url.Action(nameof(Get), new { Id = result.Id });
            return CreatedAtAction(url, await _groupChatRepository.Get(userId, result.Id));
        }

        [HttpPut("update-name/{groupId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateName(int groupId, [FromQuery] string newName)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _groupChatRepository.UpdateName(userId, groupId, newName);
            if (result.Id == 0) return BadRequest(result.Message);

            return NoContent();
        }

        [HttpPut("update-picture/{groupId:int}")]
        [Authorize]
        public async Task<IActionResult> UpdatePicture(int groupId, [FromForm] UpdateProfilePictureUrlDTO groupPicture)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _groupChatRepository.UpdatePicture(userId, groupPicture.Image, groupId);
            if (result.Id == 0) return BadRequest(result.Message);

            return NoContent();
        }

        [HttpDelete("{groupId:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int groupId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _groupChatRepository.Delete(userId, groupId);
            if (result.Id == 0) return BadRequest(result.Message);

            return NoContent();
        }

        [HttpGet("{groupId:int}")]
        [Authorize]
        public async Task<IActionResult> Get(int groupId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _groupChatRepository.Get(userId, groupId);
            if (result == null) return NotFound("id is not valis");

            return Ok(result);
        }

        [HttpGet("with-messages/{groupId:int}")]
        [Authorize]
        public async Task<IActionResult> GetWithMessages(int groupId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _groupChatRepository.GetWithMessages(userId, groupId);
            if (result == null) return NotFound("id is not valis");

            return Ok(result);
        }
    }
}

