using AssoInternesBrest.API.DTOs.BureauMembers;
using AssoInternesBrest.API.Entities;
using AutoMapper;

namespace AssoInternesBrest.API.Mappings
{
    public class BureauMemberProfile : Profile
    {
        public BureauMemberProfile()
        {
            CreateMap<BureauMember, BureauMemberDto>()
                .ForMember(dest => dest.ImageUrl,
                    opt => opt.MapFrom(src => src.Image != null ? src.Image.FilePath : null));

            CreateMap<CreateBureauMemberDto, BureauMember>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Image, opt => opt.Ignore());
        }
    }
}
