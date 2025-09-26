using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.Interface;
using System.Security.Claims;

namespace SocialMediaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendShipController : ControllerBase
    {
        private readonly IFriendShipRepository _friendShipRepository;
        public FriendShipController(IFriendShipRepository friendShipRepository)
        {
            _friendShipRepository = friendShipRepository;
        }
        [HttpGet("{Id:int}")]
        [Authorize]
        public async Task<ActionResult> GetById(int Id)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not logged in.");
            var result = await _friendShipRepository.GetById(Id, userId);
            if (result is null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        [HttpGet("FriendsYouHave")]
        [Authorize]
        public async Task<ActionResult> GetFriendsUserHave()
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _friendShipRepository.GetAllFriendShipsForUser(userId);
            return Ok(result);
        }
        [HttpPost("{Id}")]
        [Authorize]
        public async Task<ActionResult> AddById(int Id)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _friendShipRepository.Add(userId, Id);
            if (result.Id == 0)
            {
                return BadRequest(result.Message);
            }
            var url = Url.Action(nameof(GetById), new { Id = result.Id });
            return Created(url,await _friendShipRepository.GetById(result.Id, userId));
        }
        [HttpPost("from-friend-page/{friendId}")]
        [Authorize]
        public async Task<ActionResult> Add(string friendId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _friendShipRepository.Add(userId, friendId);
            if (result.Id == 0)
            {
                return BadRequest(result.Message);
            }
            var url = Url.Action(nameof(GetById), new { Id = result.Id });
            return Created(url, await _friendShipRepository.GetById(result.Id, userId));
        }
        [HttpDelete("{Id:int}")]
        [Authorize]
        public async Task<ActionResult> DeleteByID(int Id)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _friendShipRepository.Delete(Id, userId);
            if (result.Id == 0)
            {
                return BadRequest(result.Message);
            }
            return NoContent();
        }
        [HttpDelete("DeleteFromFriendPage/{FriendId}")]
        [Authorize]
        public async Task<ActionResult> Delete(string FriendId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _friendShipRepository.RemoveFriendShipFromUserPage(userId, FriendId);
            if (result.Id == 0)
            {
                return BadRequest(result.Message);
            }
            return NoContent();
        }
    }
}
