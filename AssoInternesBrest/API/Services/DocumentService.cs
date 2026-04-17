namespace AssoInternesBrest.API.Services
{
    public class DocumentService : IDocumentService
    {
        private static readonly string[] AllowedTypes =
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "text/plain",
        };

        private const long MaxFileSize = 25 * 1024 * 1024; // 25 MB

        public async Task<UploadedDocument> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded");

            if (!AllowedTypes.Contains(file.ContentType))
                throw new ArgumentException("Invalid file type");

            if (file.Length > MaxFileSize)
                throw new ArgumentException("File too large");

            string uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "uploads",
                "documents"
            );

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string safeOriginal = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{Guid.NewGuid():N}{extension}";

            string filePath = Path.Combine(uploadsFolder, fileName);
            using FileStream stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return new UploadedDocument
            {
                Url = $"/uploads/documents/{fileName}",
                OriginalName = file.FileName,
                Size = file.Length,
                MimeType = file.ContentType,
            };
        }
    }
}
