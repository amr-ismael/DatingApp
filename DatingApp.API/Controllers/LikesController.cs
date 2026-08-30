using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DatingApp.API.Dtos;
using DatingApp.API.Services;
using DatingApp.API.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LikesController : ControllerBase
    {
        private readonly ILikesService _likesService;

        public LikesController(ILikesService likesService)
        {
            _likesService = likesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLikes([FromQuery] string cursor, [FromQuery] int pageSize = 10)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var (likes, nextCursor) = await _likesService.GetLikes(userId, cursor, pageSize);

            return Ok(new PagedResponse<ListLikeDto>(likes, nextCursor, pageSize));
        }

        [HttpPost("~/api/users/{id}/like/{recipientId}")]
        public async Task<IActionResult> CreateLike(Guid id, Guid recipientId)
        {
            var callerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (id != callerId)
            {
                return Forbid();
            }

            var result = await _likesService.CreateLike(callerId, recipientId);

            if (result.IsFailure)
            {
                if (result.Error.Code == Error.Errors.Users.NotFound().Code)
                {
                    return NotFound(result.Error);
                }

                if (result.Error.Code == Error.Errors.Likes.AlreadyLiked().Code)
                {
                    return Conflict(result.Error);
                }

                return BadRequest(result.Error);
            }

            return StatusCode(201, new { isMatch = result.Value });
        }
    }
}
