using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using DatingApp.API.Shared;

namespace DatingApp.API.Services
{
    public interface IMatchesService
    {
        Task<(IEnumerable<ListMatchDto> Matches, string NextCursor)> GetMatches(int userId, string cursor, int pageSize);
        Task<Result<Match>> Unmatch(int matchId, int callerId);
        Task<Match> CreateMatch(int userAId, int userBId);
    }

    public class MatchesService : IMatchesService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IMapper _mapper;

        public MatchesService(IMatchRepository matchRepository, IMapper mapper)
        {
            _matchRepository = matchRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<ListMatchDto> Matches, string NextCursor)> GetMatches(int userId, string cursor, int pageSize)
        {
            var (matches, nextCursor) = await _matchRepository.GetMatches(userId, cursor, pageSize);

            var dtos = matches.Select(m =>
            {
                var otherUser = m.LowerUserId == userId ? m.HigherUser : m.LowerUser;
                return new ListMatchDto
                {
                    MatchId = m.Id,
                    MatchedAt = m.MatchedAt,
                    User = _mapper.Map<ListUserDto>(otherUser)
                };
            });

            return (dtos, nextCursor);
        }

        public async Task<Result<Match>> Unmatch(int matchId, int callerId)
        {
            var match = await _matchRepository.GetMatch(matchId);
            if (match == null)
            {
                return Result.Failure<Match>(Error.Errors.Matches.NotFound());
            }

            if (match.LowerUserId != callerId && match.HigherUserId != callerId)
            {
                return Result.Failure<Match>(Error.Errors.Matches.NotAuthorized());
            }

            if (!match.IsActive)
            {
                return Result.Success(match);
            }

            match.IsActive = false;
            match.UnmatchedAt = DateTime.UtcNow;
            await _matchRepository.SaveAll();

            return Result.Success(match);
        }

        public async Task<Match> CreateMatch(int userAId, int userBId)
        {
            return await _matchRepository.CreateMatch(userAId, userBId);
        }
    }
}
