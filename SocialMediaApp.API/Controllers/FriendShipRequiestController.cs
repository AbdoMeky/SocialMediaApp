using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.Interface;
using System.Security.Claims;

namespace SocialMediaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendShipRequiestController : ControllerBase
    {
        private readonly IFriendShipRequiestRepository _friendShipRequiestRepository;
        public FriendShipRequiestController(IFriendShipRequiestRepository friendShipRequestRepository)
        {
            _friendShipRequiestRepository = friendShipRequestRepository;
        }
        [HttpGet("{Id:int}")]
        [Authorize]
        public async Task<ActionResult> GetById(int Id)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _friendShipRequiestRepository.GetById(Id, userId);
            if (result is null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        [HttpGet("FriendShipRequestYouHave")]
        [Authorize]
        public async Task<ActionResult> FriendShipRequestYouHave()
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _friendShipRequiestRepository.GetAllFriendShipRequestsUserReseave(userId);
            return Ok(result);
        }
        [HttpGet("FriendShipRequestYouSend")]
        [Authorize]
        public async Task<ActionResult> FriendShipRequestYouSend()
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _friendShipRequiestRepository.GetAllFriendShipRequestsUserSend(userId);
            return Ok(result);
        }
        [HttpPost("{friendId}")]
        [Authorize]
        public async Task<ActionResult> Add(string friendId)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _friendShipRequiestRepository.Add(userId, friendId);
            if (result.Id == 0)
            {
                return BadRequest(result.Message);
            }
            var url = Url.Action(nameof(GetById), new { Id = result.Id });
            return Created(url,await _friendShipRequiestRepository.GetById(result.Id, userId));
        }
        [HttpDelete("{Id:int}")]
        [Authorize]
        public async Task<ActionResult> DeleteByID(int Id)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var result = await _friendShipRequiestRepository.Delete(Id, userId);
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
            var result = await _friendShipRequiestRepository.RemoveFriendShipRequestFromUserPage(userId, FriendId);
            if (result.Id == 0)
            {
                return BadRequest(result.Message);
            }
            return NoContent();
        }
    }
}
