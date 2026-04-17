using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Services;
using Microsoft.Extensions.Configuration;

namespace AssoInternesBrest.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly IJwtService _sut;
        private readonly User _testUser;

        public JwtServiceTests()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Secret"] = "test-secret-key-minimum-32-characters-long!!",
                    ["Jwt:ExpirationHours"] = "24",
                    ["Jwt:Issuer"] = "AssoInternesBrest",
                    ["Jwt:Audience"] = "AssoInternesBrest"
                })
                .Build();

            _sut = new JwtService(config);

            _testUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Jean",
                LastName = "Dupont",
                Email = "jean.dupont@chu-brest.fr",
                PasswordHash = "hash",
                Role = UserRole.Bureau,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public void GenerateToken_ReturnsNonEmptyString()
        {
            string token = _sut.GenerateToken(_testUser);
            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void GenerateToken_ContainsCorrectClaims()
        {
            string token = _sut.GenerateToken(_testUser);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt = handler.ReadJwtToken(token);

            Assert.Equal(_testUser.Id.ToString(), jwt.Subject);
            Assert.Equal(_testUser.Email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
            Assert.Equal(_testUser.Role.ToString(), jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        public void GenerateToken_ExpiresIn24Hours()
        {
            DateTime before = DateTime.UtcNow.AddHours(24).AddSeconds(-5);
            string token = _sut.GenerateToken(_testUser);
            DateTime after = DateTime.UtcNow.AddHours(24).AddSeconds(5);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt = handler.ReadJwtToken(token);

            Assert.True(jwt.ValidTo >= before && jwt.ValidTo <= after);
        }

        [Fact]
        public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
        {
            string token = _sut.GenerateToken(_testUser);
            ClaimsPrincipal? principal = _sut.ValidateToken(token);

            Assert.NotNull(principal);
            Assert.Equal(_testUser.Id.ToString(), principal.FindFirstValue(JwtRegisteredClaimNames.Sub));
            Assert.Equal(_testUser.Email, principal.FindFirstValue(JwtRegisteredClaimNames.Email));
            Assert.Equal(_testUser.Role.ToString(), principal.FindFirstValue(ClaimTypes.Role));
        }

        [Fact]
        public void ValidateToken_WithGarbageToken_ReturnsNull()
        {
            ClaimsPrincipal? principal = _sut.ValidateToken("not.a.jwt.token");
            Assert.Null(principal);
        }

        [Fact]
        public void ValidateToken_WithEmptyString_ReturnsNull()
        {
            ClaimsPrincipal? principal = _sut.ValidateToken("");
            Assert.Null(principal);
        }
    }
}
