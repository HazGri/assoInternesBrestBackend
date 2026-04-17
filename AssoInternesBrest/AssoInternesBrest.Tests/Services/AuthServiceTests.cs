using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;
using AssoInternesBrest.API.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AssoInternesBrest.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly IAuthService _sut;
        private readonly User _activeUser;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _jwtServiceMock = new Mock<IJwtService>();
            _emailServiceMock = new Mock<IEmailService>();

            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["App:BaseUrl"] = "http://localhost:5173"
                })
                .Build();

            _sut = new AuthService(
                _userRepoMock.Object,
                _passwordServiceMock.Object,
                _jwtServiceMock.Object,
                _emailServiceMock.Object,
                config);

            _activeUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "jean@chu-brest.fr",
                PasswordHash = "hashed",
                IsActive = true,
                Role = UserRole.Membre,
                FirstName = "Jean",
                LastName = "Dupont",
                CreatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsToken()
        {
            _userRepoMock.Setup(r => r.GetByEmailAsync("jean@chu-brest.fr")).ReturnsAsync(_activeUser);
            _passwordServiceMock.Setup(p => p.Verify("password", "hashed")).Returns(true);
            _jwtServiceMock.Setup(j => j.GenerateToken(_activeUser)).Returns("jwt-token");

            string? result = await _sut.LoginAsync("jean@chu-brest.fr", "password");

            Assert.Equal("jwt-token", result);
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ReturnsNull()
        {
            _userRepoMock.Setup(r => r.GetByEmailAsync("jean@chu-brest.fr")).ReturnsAsync(_activeUser);
            _passwordServiceMock.Setup(p => p.Verify("wrong", "hashed")).Returns(false);

            string? result = await _sut.LoginAsync("jean@chu-brest.fr", "wrong");

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WithUnknownEmail_ReturnsNull()
        {
            _userRepoMock.Setup(r => r.GetByEmailAsync("unknown@chu-brest.fr")).ReturnsAsync((User?)null);

            string? result = await _sut.LoginAsync("unknown@chu-brest.fr", "password");

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WithInactiveUser_ReturnsNull()
        {
            User inactiveUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "inactive@chu-brest.fr",
                PasswordHash = "hashed",
                IsActive = false,
                Role = UserRole.Membre,
                FirstName = "Paul",
                LastName = "Martin",
                CreatedAt = DateTime.UtcNow
            };
            _userRepoMock.Setup(r => r.GetByEmailAsync("inactive@chu-brest.fr")).ReturnsAsync(inactiveUser);

            string? result = await _sut.LoginAsync("inactive@chu-brest.fr", "password");

            Assert.Null(result);
        }

        [Fact]
        public async Task ActivateAsync_WithValidToken_ReturnsTrueAndActivatesUser()
        {
            User pendingUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "new@chu-brest.fr",
                PasswordHash = "",
                IsActive = false,
                Role = UserRole.Membre,
                FirstName = "Marie",
                LastName = "Curie",
                CreatedAt = DateTime.UtcNow,
                InvitationToken = "valid-token",
                InvitationTokenExpiresAt = DateTime.UtcNow.AddHours(72)
            };
            _userRepoMock.Setup(r => r.GetByInvitationTokenAsync("valid-token")).ReturnsAsync(pendingUser);
            _passwordServiceMock.Setup(p => p.HashPassword("newpass123")).Returns("hashed-newpass");

            bool result = await _sut.ActivateAsync("valid-token", "newpass123");

            Assert.True(result);
            Assert.True(pendingUser.IsActive);
            Assert.Equal("hashed-newpass", pendingUser.PasswordHash);
            Assert.Null(pendingUser.InvitationToken);
            Assert.Null(pendingUser.InvitationTokenExpiresAt);
        }

        [Fact]
        public async Task ActivateAsync_WithExpiredToken_ReturnsFalse()
        {
            User expiredUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "expired@chu-brest.fr",
                PasswordHash = "",
                IsActive = false,
                Role = UserRole.Membre,
                FirstName = "Alice",
                LastName = "Bernard",
                CreatedAt = DateTime.UtcNow,
                InvitationToken = "expired-token",
                InvitationTokenExpiresAt = DateTime.UtcNow.AddHours(-1)
            };
            _userRepoMock.Setup(r => r.GetByInvitationTokenAsync("expired-token")).ReturnsAsync(expiredUser);

            bool result = await _sut.ActivateAsync("expired-token", "newpass123");

            Assert.False(result);
        }

        [Fact]
        public async Task ActivateAsync_WithInvalidToken_ReturnsFalse()
        {
            _userRepoMock.Setup(r => r.GetByInvitationTokenAsync("invalid")).ReturnsAsync((User?)null);

            bool result = await _sut.ActivateAsync("invalid", "newpass123");

            Assert.False(result);
        }
    }
}
