using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/documents")]
    [Authorize(Policy = "BureauOrAdmin")]
    public class DocumentsController(IDocumentService service) : ControllerBase
    {
        private readonly IDocumentService _service = service;

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                UploadedDocument document = await _service.UploadAsync(file);
                return Ok(document);
            }
            catch (ArgumentException ex)
            {
                string message = ex.Message switch
                {
                    "No file uploaded" => "Aucun fichier envoyé.",
                    "Invalid file type" => "Format non autorisé. PDF, Word, Excel ou TXT uniquement.",
                    "File too large" => "Le fichier dépasse 25 Mo.",
                    _ => ex.Message
                };
                return BadRequest(new { message });
            }
        }
    }
}
