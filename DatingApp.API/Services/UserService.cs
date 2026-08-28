using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;

namespace DatingApp.API.Services
{
    public interface IUserService
    {
        Task<IEnumerable<ListUserDto>> GetUsers();
        Task<DetailedUserDto> GetUser(int id);
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

        public async Task<DetailedUserDto> GetUser(int id)
        {
            var user = await _userRepository.GetUser(id);
            return _mapper.Map<DetailedUserDto>(user);
        }
    }
}
