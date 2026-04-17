using System.ComponentModel.DataAnnotations;

namespace AssoInternesBrest.API.DTOs.BureauMembers
{
    public class CreateBureauMemberDto
    {
        [Required]
        public string FirstName { get; set; } = null!;
        [Required]
        public string LastName { get; set; } = null!;
        [Required]
        public string Role { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public Guid? ImageId { get; set; }
    }
}
