using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Services
{
    public interface IAuthService
    {
        Task Register(RegisterUserDto registerUserDto);
        Task<string> Login(UserLoginDto userLoginDto);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IConfiguration config)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _config = config;
        }

        public async Task Register(RegisterUserDto registerUserDto)
        {
            registerUserDto.Username = registerUserDto.Username.ToLower();
            if (await _userRepository.UserExists(registerUserDto.Username))
            {
                throw new Exception("Username already exist");
            }

            _passwordHasher.CreateHash(registerUserDto.Password, out var passwordHash, out var passwordSalt);

            var userToCreate = new User
            {
                Username = registerUserDto.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            _userRepository.Add(userToCreate);
            await _userRepository.SaveAll();
        }

        public async Task<string> Login(UserLoginDto userLoginDto)
        {
            var user = await _userRepository.GetByUsername(userLoginDto.Username.ToLower());
            if (user == null)
            {
                return null;
            }

            if (!_passwordHasher.Verify(userLoginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
