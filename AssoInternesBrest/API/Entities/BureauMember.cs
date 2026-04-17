namespace AssoInternesBrest.API.Entities
{
    public class BureauMember
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public Guid? ImageId { get; set; }
        public Image? Image { get; set; }
    }
}
