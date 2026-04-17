namespace AssoInternesBrest.API.Services
{
    public class UploadedDocument
    {
        public string Url { get; set; } = null!;
        public string OriginalName { get; set; } = null!;
        public long Size { get; set; }
        public string MimeType { get; set; } = null!;
    }

    public interface IDocumentService
    {
        Task<UploadedDocument> UploadAsync(IFormFile file);
    }
}
