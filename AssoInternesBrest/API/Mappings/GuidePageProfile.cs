using AssoInternesBrest.API.DTOs.GuidePages;
using AssoInternesBrest.API.Entities;
using AutoMapper;

namespace AssoInternesBrest.API.Mappings
{
    public class GuidePageProfile : Profile
    {
        public GuidePageProfile()
        {
            CreateMap<GuidePage, GuidePageDto>();
            CreateMap<CreateGuidePageDto, GuidePage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}
