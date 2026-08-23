using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;

namespace DatingApp.API.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserForListDto>> GetUsers();
        Task<UserForDetailedDto> GetUser(int id);
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

        public async Task<IEnumerable<UserForListDto>> GetUsers()
        {
            var users = await _userRepository.GetUsers();
            return _mapper.Map<IEnumerable<UserForListDto>>(users);
        }

        public async Task<UserForDetailedDto> GetUser(int id)
        {
            var user = await _userRepository.GetUser(id);
            return _mapper.Map<UserForDetailedDto>(user);
        }
    }
}
