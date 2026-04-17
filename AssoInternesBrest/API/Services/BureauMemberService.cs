using AssoInternesBrest.API.DTOs.BureauMembers;
using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;
using AutoMapper;

namespace AssoInternesBrest.API.Services
{
    public class BureauMemberService(IBureauMemberRepository repository, IMapper mapper) : IBureauMemberService
    {
        private readonly IBureauMemberRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<BureauMemberDto>> GetAllAsync()
        {
            IEnumerable<BureauMember> members = await _repository.GetAllOrderedAsync();
            return _mapper.Map<IEnumerable<BureauMemberDto>>(members);
        }

        public async Task<BureauMemberDto> CreateAsync(CreateBureauMemberDto dto)
        {
            BureauMember member = _mapper.Map<BureauMember>(dto);
            member.Id = Guid.NewGuid();
            BureauMember created = await _repository.AddAsync(member);
            return _mapper.Map<BureauMemberDto>(created);
        }

        public async Task<BureauMemberDto?> UpdateAsync(Guid id, UpdateBureauMemberDto dto)
        {
            BureauMember? member = await _repository.GetByIdAsync(id);
            if (member == null)
                return null;
            member.FirstName = dto.FirstName;
            member.LastName = dto.LastName;
            member.Role = dto.Role;
            member.Email = dto.Email;
            member.DisplayOrder = dto.DisplayOrder;
            await _repository.UpdateAsync(member);
            return _mapper.Map<BureauMemberDto>(member);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
