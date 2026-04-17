namespace AssoInternesBrest.API.DTOs.BureauMembers
{
    public class BureauMemberDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int DisplayOrder { get; set; }
    }
}
