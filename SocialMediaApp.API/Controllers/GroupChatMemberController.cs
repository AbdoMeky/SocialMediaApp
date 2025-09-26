using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.DTO.ChatMemberDTO;
using SocialMediaApp.Core.Interface;
using System.Security.Claims;

namespace SocialMediaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupChatMemberController : ControllerBase
    {
        private readonly IGroupChatMemberRepository _groupChatMemberRepository;

        public GroupChatMemberController(IGroupChatMemberRepository groupChatMemberRepository)
        {
            _groupChatMemberRepository = groupChatMemberRepository;
        }
        [HttpPost("add")]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] AddGroupChatMemberDTO dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _groupChatMemberRepository.Add(userId, dto);
            if (result.Id == 0) return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPut("add-admin/{memberId:int}")]
        [Authorize]
        public async Task<IActionResult> AddAdminToMember(int memberId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _groupChatMemberRepository.AddAdminToMember(userId, memberId);
            if (result.Id == 0) return BadRequest(result.Message);

            return NoContent();
        }

        [HttpDelete("remove-member/{memberId:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int memberId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _groupChatMemberRepository.Delete(userId, memberId);
            if (result.Id == 0) return BadRequest(result.Message);

            return NoContent();
        }

        [HttpPut("remove-admin/{memberId:int}")]
        [Authorize]
        public async Task<IActionResult> RemoveAdminFromMember(int memberId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _groupChatMemberRepository.RemoveAdminFromMember(userId, memberId);
            if (result.Id == 0) return BadRequest(result.Message);

            return NoContent();
        }

        [HttpGet("{chatId:int}")]
        [Authorize]
        public async Task<IActionResult> GetMembersInGroupChat(int chatId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _groupChatMemberRepository.GetMembersInGroupChat(userId,chatId);
            if(result is null) return NotFound("Id is not valid.");
            return Ok(result);
        }
    }
}

