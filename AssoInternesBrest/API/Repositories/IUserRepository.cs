using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByInvitationTokenAsync(string token);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
    }
}
