using System.ComponentModel.DataAnnotations;
using AssoInternesBrest.API.Entities;

namespace AssoInternesBrest.API.DTOs.Admin
{
    public class CreateUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public UserRole Role { get; set; }
    }
}
