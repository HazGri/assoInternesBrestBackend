using AssoInternesBrest.API.Data;
using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.Repositories
{
    public class ImageRepository(AppDbContext context) : IImageRepository
    {
        private readonly AppDbContext _context = context;
        public async Task<Image> AddAsync(Image image)
        {
            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return image;
        }
    }
}
