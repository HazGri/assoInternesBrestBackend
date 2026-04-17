using AssoInternesBrest.API.DTOs.GuidePages;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/guide")]
    public class GuideController(IGuidePageService guidePageService) : ControllerBase
    {
        private readonly IGuidePageService _guidePageService = guidePageService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GuidePageDto>>> GetAll()
        {
            IEnumerable<GuidePageDto> pages = await _guidePageService.GetAllAsync();
            return Ok(pages);
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<GuidePageDto>> GetBySlug(string slug)
        {
            GuidePageDto? page = await _guidePageService.GetBySlugAsync(slug);
            if (page == null)
                return NotFound();
            return Ok(page);
        }

        [HttpPost]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult<GuidePageDto>> Create(CreateGuidePageDto dto)
        {
            GuidePageDto created = await _guidePageService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetBySlug), new { slug = created.Slug }, created);
        }

        [HttpPut("{slug}")]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult<GuidePageDto>> Update(string slug, UpdateGuidePageDto dto)
        {
            GuidePageDto? updated = await _guidePageService.UpdateAsync(slug, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{slug}")]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult> Delete(string slug)
        {
            bool deleted = await _guidePageService.DeleteAsync(slug);
            if (!deleted)
                return NotFound();
            return Ok();
        }
    }
}
