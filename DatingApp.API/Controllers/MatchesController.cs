using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var (matches, nextCursor) = await _matchesService.GetMatches(userId, cursor, pageSize);

            Response.Headers.Append("X-Next-Cursor", nextCursor ?? string.Empty);

            return Ok(matches);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Unmatch(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            try
            {
                var match = await _matchesService.Unmatch(id, userId);
                if (match == null)
                {
                    return NotFound();
                }

                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
