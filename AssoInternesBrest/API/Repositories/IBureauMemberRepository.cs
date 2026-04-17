using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Repositories
{
    public interface IBureauMemberRepository
    {
        Task<IEnumerable<BureauMember>> GetAllOrderedAsync();
        Task<BureauMember?> GetByIdAsync(Guid id);
        Task<BureauMember> AddAsync(BureauMember member);
        Task UpdateAsync(BureauMember member);
        Task<bool> DeleteAsync(Guid id);
    }
}
