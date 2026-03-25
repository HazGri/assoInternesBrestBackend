using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Repositories
{
    public interface IImageRepository
    {
        Task<Image> AddAsync(Image image);
    }
}
