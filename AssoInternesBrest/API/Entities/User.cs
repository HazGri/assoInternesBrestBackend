using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = false;

        public DateTime CreatedAt { get; set; }
    }


    public enum UserRole
    {
        Membre,
        Bureau,
        Admin
    }
}
