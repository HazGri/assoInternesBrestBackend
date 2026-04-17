using AssoInternesBrest.API.Controllers;
using AssoInternesBrest.API.DTOs.Events;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AssoInternesBrest.Tests.Controllers
{
    public class EventRegistrationTests
    {
        private readonly Mock<IEventService> _serviceMock = new();
        private readonly EventsController _controller;

        public EventRegistrationTests()
        {
            _controller = new EventsController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetRegistration_EventWithUrl_ReturnsHasRegistrationTrue()
        {
            Guid eventId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetEventByIdAsync(eventId))
                .ReturnsAsync(new EventDto { Id = eventId, HelloAssoUrl = "https://www.helloasso.com/test" });

            ActionResult<RegistrationDto> result = await _controller.GetRegistration(eventId);

            OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
            RegistrationDto dto = Assert.IsType<RegistrationDto>(ok.Value);
            Assert.True(dto.HasRegistration);
            Assert.Equal("https://www.helloasso.com/test", dto.HelloAssoUrl);
        }

        [Fact]
        public async Task GetRegistration_EventWithoutUrl_ReturnsHasRegistrationFalse()
        {
            Guid eventId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetEventByIdAsync(eventId))
                .ReturnsAsync(new EventDto { Id = eventId, HelloAssoUrl = null });

            ActionResult<RegistrationDto> result = await _controller.GetRegistration(eventId);

            OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
            RegistrationDto dto = Assert.IsType<RegistrationDto>(ok.Value);
            Assert.False(dto.HasRegistration);
            Assert.Null(dto.HelloAssoUrl);
        }

        [Fact]
        public async Task GetRegistration_UnknownEvent_Returns404()
        {
            Guid eventId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetEventByIdAsync(eventId))
                .ReturnsAsync((EventDto?)null);

            ActionResult<RegistrationDto> result = await _controller.GetRegistration(eventId);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
