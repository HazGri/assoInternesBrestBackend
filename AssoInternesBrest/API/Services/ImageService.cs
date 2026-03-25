using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.PixelFormats;

namespace AssoInternesBrest.API.Services
{
    public class ImageService(IImageRepository repository) : IImageService
    {
        private readonly IImageRepository _repository = repository;

        private static readonly string[] AllowedTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        private const int MaxWidth = 4000;
        private const int MaxHeight = 4000;

        public async Task<Image> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded");

            if (!AllowedTypes.Contains(file.ContentType))
                throw new ArgumentException("Invalid file type");

            if (file.Length > MaxFileSize)
                throw new ArgumentException("File too large");

            // vérifier dimensions
            using var imageStream = file.OpenReadStream();

            using var imageInfo = await ImageSharpImage.LoadAsync<Rgba32>(imageStream);

            if (imageInfo.Width > MaxWidth || imageInfo.Height > MaxHeight)
                throw new ArgumentException("Image dimensions too large");

            // reset stream pour sauvegarde
            imageStream.Position = 0;

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "uploads",
                "events"
            );

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);

            await file.CopyToAsync(stream);

            var image = new Image
            {
                Id = Guid.NewGuid(),
                FilePath = $"/uploads/events/{fileName}",
                OriginalName = file.FileName,
                MimeType = file.ContentType,
                Size = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            return await _repository.AddAsync(image);
        }
    }
}