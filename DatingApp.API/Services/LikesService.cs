using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Shared;

namespace DatingApp.API.Services
{
    public interface ILikesService
    {
        Task<(IEnumerable<ListLikeDto> Likes, string NextCursor)> GetLikes(Guid userId, string cursor, int pageSize);
        Task<Result<bool>> CreateLike(Guid likerId, Guid likeeId);
    }

    public class LikesService : ILikesService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMatchesService _matchesService;
        private readonly IMapper _mapper;

        public LikesService(
            ILikeRepository likeRepository,
            IUserRepository userRepository,
            IMatchesService matchesService,
            IMapper mapper)
        {
            _likeRepository = likeRepository;
            _userRepository = userRepository;
            _matchesService = matchesService;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<ListLikeDto> Likes, string NextCursor)> GetLikes(Guid userId, string cursor, int pageSize)
        {
            var (likes, nextCursor) = await _likeRepository.GetLikes(userId, cursor, pageSize);

            var dtos = likes.Select(l => new ListLikeDto
            {
                LikeId = l.Id,
                CreatedAt = l.CreatedAt,
                User = _mapper.Map<ListUserDto>(l.Likee)
            });

            return (dtos, nextCursor);
        }

        public async Task<Result<bool>> CreateLike(Guid likerId, Guid likeeId)
        {
            if (likerId == likeeId)
            {
                return Result.Failure<bool>(Error.Errors.Likes.CannotLikeSelf());
            }

            if (!await _userRepository.UserExists(likeeId))
            {
                return Result.Failure<bool>(Error.Errors.Users.NotFound());
            }

            if (await _likeRepository.GetLike(likerId, likeeId) != null)
            {
                return Result.Failure<bool>(Error.Errors.Likes.AlreadyLiked());
            }

            await _likeRepository.CreateLike(likerId, likeeId);

            var reverseLike = await _likeRepository.GetLike(likeeId, likerId);
            if (reverseLike == null)
            {
                return Result.Success(false);
            }

            await _matchesService.CreateMatch(likerId, likeeId);
            return Result.Success(true);
        }
    }
}
