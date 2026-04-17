using AssoInternesBrest.API.DTOs.Articles;
using AssoInternesBrest.API.Entities;
using AutoMapper;

namespace AssoInternesBrest.API.Mappings
{
    public class ArticleProfile : Profile
    {
        public ArticleProfile()
        {
            CreateMap<Article, ArticleDto>()
                .ForMember(dest => dest.AuthorName,
                    opt => opt.MapFrom(src => src.Author != null
                        ? $"{src.Author.FirstName} {src.Author.LastName}"
                        : ""))
                .ForMember(dest => dest.ImageUrl,
                    opt => opt.MapFrom(src => src.Image != null ? src.Image.FilePath : null));

            CreateMap<CreateArticleDto, Article>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorId, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore())
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}
