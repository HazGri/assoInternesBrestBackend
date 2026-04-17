using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Services
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(string email, string password);
        Task<User> CreateUserAsync(string email, string firstName, string lastName, UserRole role);
        Task<bool> ActivateAsync(string token, string newPassword);
    }
}
