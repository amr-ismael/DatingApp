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
    public class LikesServiceTests
    {
        private readonly Mock<ILikeRepository> _likeRepository;
        private readonly Mock<IUserRepository> _userRepository;
        private readonly Mock<IMatchesService> _matchesService;
        private readonly Mock<IMapper> _mapper;
        private readonly LikesService _service;

        public LikesServiceTests()
        {
            _likeRepository = new Mock<ILikeRepository>();
            _userRepository = new Mock<IUserRepository>();
            _matchesService = new Mock<IMatchesService>();
            _mapper = new Mock<IMapper>();
            _mapper.Setup(m => m.Map<ListUserDto>(It.IsAny<User>()))
                .Returns((User u) => new ListUserDto { Id = u.Id, Username = u.Username });

            _service = new LikesService(_likeRepository.Object, _userRepository.Object, _matchesService.Object, _mapper.Object);
        }

        [Fact]
        public async Task GetLikes_ReturnsLikeeAsTheOtherUser()
        {
            var likeId = Guid.NewGuid();
            var likerId = Guid.NewGuid();
            var likeeId = Guid.NewGuid();
            var like = new Like
            {
                Id = likeId,
                LikerId = likerId,
                LikeeId = likeeId,
                Likee = new User { Id = likeeId, Username = "dorothy" },
                CreatedAt = DateTime.UtcNow
            };
            _likeRepository.Setup(r => r.GetLikes(likerId, null, 10))
                .ReturnsAsync((new List<Like> { like }, (string)null));

            var (likes, nextCursor) = await _service.GetLikes(likerId, null, 10);

            var dto = Assert.Single(likes);
            Assert.Equal(likeId, dto.LikeId);
            Assert.Equal(likeeId, dto.User.Id);
            Assert.Null(nextCursor);
        }

        [Fact]
        public async Task GetLikes_PassesThroughNextCursorFromRepository()
        {
            var likerId = Guid.NewGuid();
            _likeRepository.Setup(r => r.GetLikes(likerId, null, 10))
                .ReturnsAsync((new List<Like>(), "some-cursor"));

            var (_, nextCursor) = await _service.GetLikes(likerId, null, 10);

            Assert.Equal("some-cursor", nextCursor);
        }

        [Fact]
        public async Task CreateLike_LikerEqualsLikee_ReturnsCannotLikeSelfFailure()
        {
            var userId = Guid.NewGuid();

            var result = await _service.CreateLike(userId, userId);

            Assert.True(result.IsFailure);
            Assert.Equal(Error.Errors.Likes.CannotLikeSelf().Code, result.Error.Code);
            _likeRepository.Verify(r => r.CreateLike(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CreateLike_LikeeDoesNotExist_ReturnsUserNotFoundFailure()
        {
            var likerId = Guid.NewGuid();
            var likeeId = Guid.NewGuid();
            _userRepository.Setup(r => r.UserExists(likeeId)).ReturnsAsync(false);

            var result = await _service.CreateLike(likerId, likeeId);

            Assert.True(result.IsFailure);
            Assert.Equal(Error.Errors.Users.NotFound().Code, result.Error.Code);
            _likeRepository.Verify(r => r.CreateLike(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CreateLike_AlreadyLiked_ReturnsAlreadyLikedFailure()
        {
            var likerId = Guid.NewGuid();
            var likeeId = Guid.NewGuid();
            _userRepository.Setup(r => r.UserExists(likeeId)).ReturnsAsync(true);
            _likeRepository.Setup(r => r.GetLike(likerId, likeeId)).ReturnsAsync(new Like { Id = Guid.NewGuid(), LikerId = likerId, LikeeId = likeeId });

            var result = await _service.CreateLike(likerId, likeeId);

            Assert.True(result.IsFailure);
            Assert.Equal(Error.Errors.Likes.AlreadyLiked().Code, result.Error.Code);
            _likeRepository.Verify(r => r.CreateLike(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CreateLike_NoReverseLike_CreatesLikeAndReturnsIsMatchFalse()
        {
            var likerId = Guid.NewGuid();
            var likeeId = Guid.NewGuid();
            _userRepository.Setup(r => r.UserExists(likeeId)).ReturnsAsync(true);
            _likeRepository.Setup(r => r.GetLike(likerId, likeeId)).ReturnsAsync((Like)null);
            _likeRepository.Setup(r => r.GetLike(likeeId, likerId)).ReturnsAsync((Like)null);

            var result = await _service.CreateLike(likerId, likeeId);

            Assert.True(result.IsSuccessful);
            Assert.False(result.Value);
            _likeRepository.Verify(r => r.CreateLike(likerId, likeeId), Times.Once);
            _matchesService.Verify(m => m.CreateMatch(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CreateLike_ReverseLikeExists_CreatesMatchAndReturnsIsMatchTrue()
        {
            var likerId = Guid.NewGuid();
            var likeeId = Guid.NewGuid();
            _userRepository.Setup(r => r.UserExists(likeeId)).ReturnsAsync(true);
            _likeRepository.Setup(r => r.GetLike(likerId, likeeId)).ReturnsAsync((Like)null);
            _likeRepository.Setup(r => r.GetLike(likeeId, likerId)).ReturnsAsync(new Like { Id = Guid.NewGuid(), LikerId = likeeId, LikeeId = likerId });

            var result = await _service.CreateLike(likerId, likeeId);

            Assert.True(result.IsSuccessful);
            Assert.True(result.Value);
            _likeRepository.Verify(r => r.CreateLike(likerId, likeeId), Times.Once);
            _matchesService.Verify(m => m.CreateMatch(likerId, likeeId), Times.Once);
        }
    }
}
