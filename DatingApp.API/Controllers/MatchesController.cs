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
    public class MatchesController : ControllerBase
    {
        private readonly IMatchesService _matchesService;

        public MatchesController(IMatchesService matchesService)
        {
            _matchesService = matchesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMatches([FromQuery] string cursor, [FromQuery] int pageSize = 10)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var (matches, nextCursor) = await _matchesService.GetMatches(userId, cursor, pageSize);

            return Ok(new PagedResponse<ListMatchDto>(matches, nextCursor, pageSize));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Unmatch(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var result = await _matchesService.Unmatch(id, userId);

            if (result.IsFailure)
            {
                return result.Error.Code == Error.Errors.Matches.NotFound().Code
                    ? NotFound(result.Error)
                    : Forbid();
            }

            return Ok();
        }
    }
}
