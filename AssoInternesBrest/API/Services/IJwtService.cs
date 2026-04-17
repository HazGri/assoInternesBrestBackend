using System.Security.Claims;
using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
