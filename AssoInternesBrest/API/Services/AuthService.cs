using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;

namespace AssoInternesBrest.API.Services
{
    public class AuthService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtService jwtService,
        IEmailService emailService,
        IConfiguration configuration) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPasswordService _passwordService = passwordService;
        private readonly IJwtService _jwtService = jwtService;
        private readonly IEmailService _emailService = emailService;
        private readonly IConfiguration _configuration = configuration;

        public async Task<string?> LoginAsync(string email, string password)
        {
            User? user = await _userRepository.GetByEmailAsync(email);
            if (user == null || !user.IsActive)
                return null;
            if (!_passwordService.Verify(password, user.PasswordHash))
                return null;
            return _jwtService.GenerateToken(user);
        }

        public async Task<User> CreateUserAsync(string email, string firstName, string lastName, UserRole role)
        {
            User? existing = await _userRepository.GetByEmailAsync(email);
            if (existing != null)
                throw new InvalidOperationException("EMAIL_EXISTS");

            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                PasswordHash = "",
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                InvitationToken = Guid.NewGuid().ToString("N"),
                InvitationTokenExpiresAt = DateTime.UtcNow.AddHours(72)
            };

            await _userRepository.AddAsync(user);

            string activationUrl = $"{_configuration["App:BaseUrl"]}/activate?token={user.InvitationToken}";
            string body = $"Bonjour {firstName},\n\nVotre compte a été créé sur le site de l'Asso Internes Brest.\n\nCliquez sur le lien suivant pour définir votre mot de passe (valable 72h) :\n\n{activationUrl}\n\nL'équipe Asso Internes Brest";
            await _emailService.SendAsync(email, "Activation de votre compte — Asso Internes Brest", body);

            return user;
        }

        public async Task<bool> ActivateAsync(string token, string newPassword)
        {
            User? user = await _userRepository.GetByInvitationTokenAsync(token);
            if (user == null || user.InvitationTokenExpiresAt < DateTime.UtcNow)
                return false;

            user.PasswordHash = _passwordService.HashPassword(newPassword);
            user.IsActive = true;
            user.InvitationToken = null;
            user.InvitationTokenExpiresAt = null;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            User? user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
                return false;
            if (!_passwordService.Verify(currentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = _passwordService.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}
