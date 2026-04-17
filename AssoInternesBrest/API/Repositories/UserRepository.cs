using AssoInternesBrest.API.Data;
using AssoInternesBrest.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Repositories
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByInvitationTokenAsync(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.InvitationToken == token);
        }

        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
