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
            var match = new Match
            {
                Id = 1,
                LowerUserId = 1,
                HigherUser = new User { Id = 2, Username = "dorothy" },
                HigherUserId = 2,
                LowerUser = new User { Id = 1, Username = "lola" },
                MatchedAt = DateTime.UtcNow,
                IsActive = true
            };
            _matchRepository.Setup(r => r.GetMatches(1, null, 10))
                .ReturnsAsync((new List<Match> { match }, (string)null));

            var (matches, nextCursor) = await _service.GetMatches(1, null, 10);

            var dto = Assert.Single(matches);
            Assert.Equal(1, dto.MatchId);
            Assert.Equal(2, dto.User.Id);
            Assert.Equal("dorothy", dto.User.Username);
            Assert.Null(nextCursor);
        }

        [Fact]
        public async Task GetMatches_CallerIsHigherUser_ReturnsLowerUserAsTheOtherUser()
        {
            var match = new Match
            {
                Id = 1,
                LowerUserId = 1,
                LowerUser = new User { Id = 1, Username = "lola" },
                HigherUserId = 2,
                HigherUser = new User { Id = 2, Username = "dorothy" },
                MatchedAt = DateTime.UtcNow,
                IsActive = true
            };
            _matchRepository.Setup(r => r.GetMatches(2, null, 10))
                .ReturnsAsync((new List<Match> { match }, (string)null));

            var (matches, _) = await _service.GetMatches(2, null, 10);

            var dto = Assert.Single(matches);
            Assert.Equal(1, dto.User.Id);
            Assert.Equal("lola", dto.User.Username);
        }

        [Fact]
        public async Task GetMatches_PassesThroughNextCursorFromRepository()
        {
            _matchRepository.Setup(r => r.GetMatches(1, null, 10))
                .ReturnsAsync((new List<Match>(), "some-cursor"));

            var (_, nextCursor) = await _service.GetMatches(1, null, 10);

            Assert.Equal("some-cursor", nextCursor);
        }

        [Fact]
        public async Task Unmatch_MatchNotFound_ReturnsNotFoundFailure()
        {
            _matchRepository.Setup(r => r.GetMatch(1)).ReturnsAsync((Match)null);

            var result = await _service.Unmatch(1, 1);

            Assert.True(result.IsFailure);
            Assert.Equal(Error.Errors.Matches.NotFound().Code, result.Error.Code);
            _matchRepository.Verify(r => r.SaveAll(), Times.Never);
        }

        [Fact]
        public async Task Unmatch_CallerNotAParticipant_ReturnsNotAuthorizedFailure()
        {
            var match = new Match { Id = 1, LowerUserId = 1, HigherUserId = 2, IsActive = true };
            _matchRepository.Setup(r => r.GetMatch(1)).ReturnsAsync(match);

            var result = await _service.Unmatch(1, 99);

            Assert.True(result.IsFailure);
            Assert.Equal(Error.Errors.Matches.NotAuthorized().Code, result.Error.Code);
            _matchRepository.Verify(r => r.SaveAll(), Times.Never);
        }

        [Fact]
        public async Task Unmatch_AlreadyInactive_IsIdempotentAndDoesNotSave()
        {
            var match = new Match { Id = 1, LowerUserId = 1, HigherUserId = 2, IsActive = false, UnmatchedAt = DateTime.UtcNow.AddDays(-1) };
            _matchRepository.Setup(r => r.GetMatch(1)).ReturnsAsync(match);

            var result = await _service.Unmatch(1, 1);

            Assert.True(result.IsSuccessful);
            Assert.Same(match, result.Value);
            _matchRepository.Verify(r => r.SaveAll(), Times.Never);
        }

        [Fact]
        public async Task Unmatch_ActiveMatch_CallerIsLowerUser_SoftDeletesAndSaves()
        {
            var match = new Match { Id = 1, LowerUserId = 1, HigherUserId = 2, IsActive = true };
            _matchRepository.Setup(r => r.GetMatch(1)).ReturnsAsync(match);
            _matchRepository.Setup(r => r.SaveAll()).ReturnsAsync(true);

            var result = await _service.Unmatch(1, 1);

            Assert.True(result.IsSuccessful);
            Assert.False(result.Value.IsActive);
            Assert.NotNull(result.Value.UnmatchedAt);
            _matchRepository.Verify(r => r.SaveAll(), Times.Once);
        }

        [Fact]
        public async Task Unmatch_ActiveMatch_CallerIsHigherUser_SoftDeletesAndSaves()
        {
            var match = new Match { Id = 1, LowerUserId = 1, HigherUserId = 2, IsActive = true };
            _matchRepository.Setup(r => r.GetMatch(1)).ReturnsAsync(match);
            _matchRepository.Setup(r => r.SaveAll()).ReturnsAsync(true);

            var result = await _service.Unmatch(1, 2);

            Assert.True(result.IsSuccessful);
            Assert.False(result.Value.IsActive);
            Assert.NotNull(result.Value.UnmatchedAt);
        }

        [Fact]
        public async Task CreateMatch_DelegatesToRepository()
        {
            var created = new Match { Id = 5, LowerUserId = 1, HigherUserId = 2 };
            _matchRepository.Setup(r => r.CreateMatch(2, 1)).ReturnsAsync(created);

            var result = await _service.CreateMatch(2, 1);

            Assert.Same(created, result);
        }
    }
}
