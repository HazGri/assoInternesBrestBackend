using AssoInternesBrest.API.Controllers;
using AssoInternesBrest.API.DTOs.Contact;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AssoInternesBrest.Tests.Controllers
{
    public class ContactControllerTests
    {
        private readonly Mock<IEmailService> _emailMock = new();
        private readonly Mock<IAppSettingService> _settingsMock = new();
        private readonly ContactController _controller;

        public ContactControllerTests()
        {
            _controller = new ContactController(_emailMock.Object, _settingsMock.Object);
        }

        [Fact]
        public async Task SendContact_ValidDto_Returns200()
        {
            _settingsMock.Setup(s => s.GetValueAsync("contact_email"))
                .ReturnsAsync("contact@asso-internes-brest.fr");

            ContactDto dto = new() { Name = "Alice", Email = "alice@example.com", Message = "Hello!" };

            ActionResult result = await _controller.SendContact(dto);

            Assert.IsType<OkResult>(result);
            _emailMock.Verify(e => e.SendAsync(
                "contact@asso-internes-brest.fr",
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SendContact_EmailServiceThrows_Returns500()
        {
            _settingsMock.Setup(s => s.GetValueAsync("contact_email"))
                .ReturnsAsync("contact@asso-internes-brest.fr");
            _emailMock.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP error"));

            ContactDto dto = new() { Name = "Alice", Email = "alice@example.com", Message = "Hello!" };

            ActionResult result = await _controller.SendContact(dto);

            Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, ((ObjectResult)result).StatusCode);
        }
    }
}
