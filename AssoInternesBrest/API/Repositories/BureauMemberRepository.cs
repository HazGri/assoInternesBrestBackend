using AssoInternesBrest.API.Data;
using AssoInternesBrest.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Repositories
{
    public class BureauMemberRepository(AppDbContext context) : IBureauMemberRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<IEnumerable<BureauMember>> GetAllOrderedAsync()
        {
            return await _context.BureauMembers
                .Include(b => b.Image)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();
        }

        public async Task<BureauMember?> GetByIdAsync(Guid id)
        {
            return await _context.BureauMembers
                .Include(b => b.Image)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<BureauMember> AddAsync(BureauMember member)
        {
            _context.BureauMembers.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public async Task UpdateAsync(BureauMember member)
        {
            _context.BureauMembers.Update(member);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            BureauMember? member = await _context.BureauMembers.FindAsync(id);
            if (member == null)
                return false;
            _context.BureauMembers.Remove(member);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
