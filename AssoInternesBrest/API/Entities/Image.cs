namespace AssoInternesBrest.API.Entities
{
    public class Image
    {
        public Guid Id { get; set; }

        public string FilePath { get; set; } = null!;

        public string OriginalName { get; set; } = null!;

        public string MimeType { get; set; } = null!;

        public long Size { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}
