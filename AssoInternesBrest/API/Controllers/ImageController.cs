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
            try
            {
                Entities.Image image = await _service.UploadAsync(file);
                return Ok(image);
            }
            catch (ArgumentException ex)
            {
                string message = ex.Message switch
                {
                    "No file uploaded" => "Aucun fichier envoyé.",
                    "Invalid file type" => "Format non autorisé. JPEG, PNG ou WebP uniquement.",
                    "File too large" => "Le fichier dépasse 5 Mo.",
                    "Image dimensions too large" => "Les dimensions de l'image dépassent 4000 × 4000 px.",
                    _ => ex.Message
                };
                return BadRequest(new { message });
            }
        }
    }
}
