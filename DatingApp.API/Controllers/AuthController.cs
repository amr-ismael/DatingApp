using System.Threading.Tasks;
using DatingApp.API.Dtos;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto userForRegisterDto)
        {
            var result = await _authService.Register(userForRegisterDto);
            if (result.IsFailure)
            {
                return Conflict(result.Error);
            }

            return new StatusCodeResult(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userForLoginDto)
        {
            var result = await _authService.Login(userForLoginDto);
            if (result.IsFailure)
            {
                return Unauthorized(result.Error);
            }

            return Ok(new { token = result.Value });
        }
    }
}
