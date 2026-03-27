namespace AssoInternesBrest.API.Services
{
    public interface IPasswordService
    {
        public string HashPassword(string password);

        public bool Verify(string password, string hash);
    }
}
