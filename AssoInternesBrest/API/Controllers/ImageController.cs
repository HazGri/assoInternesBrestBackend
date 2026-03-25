using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImagesController(IImageService service) : ControllerBase
    {
        private readonly IImageService _service = service;

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var image = await _service.UploadAsync(file);

            return Ok(image);
        }
    }
}
