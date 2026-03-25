using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Services
{
    public interface IImageService
    {
        Task<Image> UploadAsync(IFormFile file);
    }
}
