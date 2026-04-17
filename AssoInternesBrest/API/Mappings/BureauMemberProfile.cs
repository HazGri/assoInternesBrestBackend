using AssoInternesBrest.API.DTOs.BureauMembers;
using AssoInternesBrest.API.Entities;
using AutoMapper;

namespace AssoInternesBrest.API.Mappings
{
    public class BureauMemberProfile : Profile
    {
        public BureauMemberProfile()
        {
            CreateMap<BureauMember, BureauMemberDto>();
            CreateMap<CreateBureauMemberDto, BureauMember>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
