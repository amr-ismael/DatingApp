using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using DatingApp.API.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Services
{
    public interface IAuthService
    {
        Task<Result> Register(RegisterUserDto registerUserDto);
        Task<Result<string>> Login(LoginUserDto userLoginDto);
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

        public async Task<Result> Register(RegisterUserDto registerUserDto)
        {
            var email = registerUserDto.Email.ToLower();
            if (await _userRepository.EmailExists(email))
            {
                return Result.Failure(Error.Errors.Auth.EmailTaken());
            }

            _passwordHasher.CreateHash(registerUserDto.Password, out var passwordHash, out var passwordSalt);

            var userToCreate = new User
            {
                Username = Guid.NewGuid().ToString(),
                FirstName = registerUserDto.FirstName,
                LastName = registerUserDto.LastName,
                Email = email,
                DateOfBirth = registerUserDto.DateOfBirth,
                Gender = registerUserDto.Gender.Value,
                InterestedIn = registerUserDto.InterestedIn.Value,
                Location = registerUserDto.Location,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Created = DateTime.UtcNow,
                LastActive = DateTime.UtcNow
            };

            _userRepository.Add(userToCreate);
            await _userRepository.SaveAll();

            return Result.Success();
        }

        public async Task<Result<string>> Login(LoginUserDto userLoginDto)
        {
            var user = await _userRepository.GetByEmail(userLoginDto.Email.ToLower());
            if (user == null)
            {
                return Result.Failure<string>(Error.Errors.Auth.InvalidCredentials());
            }

            if (!_passwordHasher.Verify(userLoginDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                return Result.Failure<string>(Error.Errors.Auth.InvalidCredentials());
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
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

            return Result.Success(tokenHandler.WriteToken(token));
        }
    }
}
