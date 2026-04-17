using AssoInternesBrest.API.DTOs.BureauMembers;

namespace AssoInternesBrest.API.Services
{
    public interface IBureauMemberService
    {
        Task<IEnumerable<BureauMemberDto>> GetAllAsync();
        Task<BureauMemberDto> CreateAsync(CreateBureauMemberDto dto);
        Task<BureauMemberDto?> UpdateAsync(Guid id, UpdateBureauMemberDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
