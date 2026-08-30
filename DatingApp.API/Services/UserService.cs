using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Shared;

namespace DatingApp.API.Services
{
    public interface IUserService
    {
        Task<IEnumerable<ListUserDto>> GetUsers();
        Task<Result<DetailedUserDto>> GetUser(Guid id);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ListUserDto>> GetUsers()
        {
            var users = await _userRepository.GetUsers();
            return _mapper.Map<IEnumerable<ListUserDto>>(users);
        }

        public async Task<Result<DetailedUserDto>> GetUser(Guid id)
        {
            var user = await _userRepository.GetUser(id);
            if (user == null)
            {
                return Result.Failure<DetailedUserDto>(Error.Errors.Users.NotFound());
            }

            return Result.Success(_mapper.Map<DetailedUserDto>(user));
        }
    }
}
