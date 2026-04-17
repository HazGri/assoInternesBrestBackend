using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Repositories;
using AssoInternesBrest.API.Services;
using Moq;

namespace AssoInternesBrest.Tests.Services
{
    public class AppSettingServiceTests
    {
        private readonly Mock<IAppSettingRepository> _repoMock = new();
        private readonly AppSettingService _service;

        public AppSettingServiceTests()
        {
            _service = new AppSettingService(_repoMock.Object);
        }

        [Fact]
        public async Task GetValueAsync_ExistingKey_ReturnsValue()
        {
            _repoMock.Setup(r => r.GetByKeyAsync("contact_email"))
                .ReturnsAsync(new AppSetting { Key = "contact_email", Value = "test@example.com" });

            string? result = await _service.GetValueAsync("contact_email");

            Assert.Equal("test@example.com", result);
        }

        [Fact]
        public async Task GetValueAsync_MissingKey_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByKeyAsync("missing")).ReturnsAsync((AppSetting?)null);

            string? result = await _service.GetValueAsync("missing");

            Assert.Null(result);
        }

        [Fact]
        public async Task SetValueAsync_CallsUpsert()
        {
            await _service.SetValueAsync("contact_email", "new@example.com");

            _repoMock.Verify(r => r.UpsertAsync(It.Is<AppSetting>(s =>
                s.Key == "contact_email" && s.Value == "new@example.com")), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllSettings()
        {
            List<AppSetting> settings = new()
            {
                new AppSetting { Key = "contact_email", Value = "a@b.com" }
            };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(settings);

            IEnumerable<AppSetting> result = await _service.GetAllAsync();

            Assert.Single(result);
        }
    }
}
