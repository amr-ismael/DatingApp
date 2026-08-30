using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using DatingApp.API.Services;
using DatingApp.API.Shared;
using Moq;
using Xunit;
using Match = DatingApp.API.Models.Match;

namespace DatingApp.API.Tests.Services
{
    public class MatchesServiceTests
    {
        private readonly Mock<IMatchRepository> _matchRepository;
        private readonly Mock<IMapper> _mapper;
        private readonly MatchesService _service;

        public MatchesServiceTests()
        {
            _matchRepository = new Mock<IMatchRepository>();
            _mapper = new Mock<IMapper>();
            _mapper.Setup(m => m.Map<ListUserDto>(It.IsAny<User>()))
                .Returns((User u) => new ListUserDto { Id = u.Id, Username = u.Username });

            _service = new MatchesService(_matchRepository.Object, _mapper.Object);
        }

        [Fact]
        public async Task GetMatches_CallerIsLowerUser_ReturnsHigherUserAsTheOtherUser()
        {
            var matchId = Guid.NewGuid();
            var lowerUserId = Guid.NewGuid();
            var higherUserId = Guid.NewGuid();
            var match = new Match
            {
                Id = matchId,
                LowerUserId = lowerUserId,
                HigherUser = new User { Id = higherUserId, Username = "dorothy" },
                HigherUserId = higherUserId,
                LowerUser = new User { Id = lowerUserId, Username = "lola" },
                MatchedAt = DateTime.UtcNow,
                IsActive = true
            };
            _matchRepository.Setup(r => r.GetMatches(lowerUserId, null, 10))
                .ReturnsAsync((new List<Match> { match }, (string)null));

            var (matches, nextCursor) = await _service.GetMatches(lowerUserId, null, 10);

            var dto = Assert.Single(matches);
            Assert.Equal(matchId, dto.MatchId);
            Assert.Equal(higherUserId, dto.User.Id);
            Assert.Equal("dorothy", dto.User.Username);
            Assert.Null(nextCursor);
        }

        [Fact]
        public async Task GetMatches_CallerIsHigherUser_ReturnsLowerUserAsTheOtherUser()
        {
            var matchId = Guid.NewGuid();
            var lowerUserId = Guid.NewGuid();
            var higherUserId = Guid.NewGuid();
            var match = new Match
            {
                Id = matchId,
                LowerUserId = lowerUserId,
                LowerUser = new User { Id = lowerUserId, Username = "lola" },
                HigherUserId = higherUserId,
                HigherUser = new User { Id = higherUserId, Username = "dorothy" },
                MatchedAt = DateTime.UtcNow,
                IsActive = true
            };
            _matchRepository.Setup(r => r.GetMatches(higherUserId, null, 10))
                .ReturnsAsync((new List<Match> { match }, (string)null));

            var (matches, _) = await _service.GetMatches(higherUserId, null, 10);

            var dto = Assert.Single(matches);
            Assert.Equal(lowerUserId, dto.User.Id);
            Assert.Equal("lola", dto.User.Username);
        }

        [Fact]
        public async Task GetMatches_PassesThroughNextCursorFromRepository()
        {
            var userId = Guid.NewGuid();
            _matchRepository.Setup(r => r.GetMatches(userId, null, 10))
                .ReturnsAsync((new List<Match>(), "some-cursor"));

            var (_, nextCursor) = await _service.GetMatches(userId, null, 10);

            Assert.Equal("some-cursor", nextCursor);
        }

        [Fact]
        public async Task Unmatch_MatchNotFound_ReturnsNotFoundFailure()
        {
            var matchId = Guid.NewGuid();
            var callerId = Guid.NewGuid();
            _matchRepository.Setup(r => r.GetMatch(matchId)).ReturnsAsync((Match)null);

            var result = await _service.Unmatch(matchId, callerId);

            Assert.True(result.IsFailure);
            Assert.Equal(Error.Errors.Matches.NotFound().Code, result.Error.Code);
            _matchRepository.Verify(r => r.SaveAll(), Times.Never);
        }

        [Fact]
        public async Task Unmatch_CallerNotAParticipant_ReturnsNotAuthorizedFailure()
        {
            var matchId = Guid.NewGuid();
            var lowerUserId = Guid.NewGuid();
            var higherUserId = Guid.NewGuid();
            var unrelatedUserId = Guid.NewGuid();
            var match = new Match { Id = matchId, LowerUserId = lowerUserId, HigherUserId = higherUserId, IsActive = true };
            _matchRepository.Setup(r => r.GetMatch(matchId)).ReturnsAsync(match);

            var result = await _service.Unmatch(matchId, unrelatedUserId);

            Assert.True(result.IsFailure);
            Assert.Equal(Error.Errors.Matches.NotAuthorized().Code, result.Error.Code);
            _matchRepository.Verify(r => r.SaveAll(), Times.Never);
        }

        [Fact]
        public async Task Unmatch_AlreadyInactive_IsIdempotentAndDoesNotSave()
        {
            var matchId = Guid.NewGuid();
            var lowerUserId = Guid.NewGuid();
            var higherUserId = Guid.NewGuid();
            var match = new Match { Id = matchId, LowerUserId = lowerUserId, HigherUserId = higherUserId, IsActive = false, UnmatchedAt = DateTime.UtcNow.AddDays(-1) };
            _matchRepository.Setup(r => r.GetMatch(matchId)).ReturnsAsync(match);

            var result = await _service.Unmatch(matchId, lowerUserId);

            Assert.True(result.IsSuccessful);
            Assert.Same(match, result.Value);
            _matchRepository.Verify(r => r.SaveAll(), Times.Never);
        }

        [Fact]
        public async Task Unmatch_ActiveMatch_CallerIsLowerUser_SoftDeletesAndSaves()
        {
            var matchId = Guid.NewGuid();
            var lowerUserId = Guid.NewGuid();
            var higherUserId = Guid.NewGuid();
            var match = new Match { Id = matchId, LowerUserId = lowerUserId, HigherUserId = higherUserId, IsActive = true };
            _matchRepository.Setup(r => r.GetMatch(matchId)).ReturnsAsync(match);
            _matchRepository.Setup(r => r.SaveAll()).ReturnsAsync(true);

            var result = await _service.Unmatch(matchId, lowerUserId);

            Assert.True(result.IsSuccessful);
            Assert.False(result.Value.IsActive);
            Assert.NotNull(result.Value.UnmatchedAt);
            _matchRepository.Verify(r => r.SaveAll(), Times.Once);
        }

        [Fact]
        public async Task Unmatch_ActiveMatch_CallerIsHigherUser_SoftDeletesAndSaves()
        {
            var matchId = Guid.NewGuid();
            var lowerUserId = Guid.NewGuid();
            var higherUserId = Guid.NewGuid();
            var match = new Match { Id = matchId, LowerUserId = lowerUserId, HigherUserId = higherUserId, IsActive = true };
            _matchRepository.Setup(r => r.GetMatch(matchId)).ReturnsAsync(match);
            _matchRepository.Setup(r => r.SaveAll()).ReturnsAsync(true);

            var result = await _service.Unmatch(matchId, higherUserId);

            Assert.True(result.IsSuccessful);
            Assert.False(result.Value.IsActive);
            Assert.NotNull(result.Value.UnmatchedAt);
        }

        [Fact]
        public async Task CreateMatch_DelegatesToRepository()
        {
            var userAId = Guid.NewGuid();
            var userBId = Guid.NewGuid();
            var created = new Match { Id = Guid.NewGuid(), LowerUserId = userBId, HigherUserId = userAId };
            _matchRepository.Setup(r => r.CreateMatch(userAId, userBId)).ReturnsAsync(created);

            var result = await _service.CreateMatch(userAId, userBId);

            Assert.Same(created, result);
        }
    }
}
