using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AssoInternesBrest.API.DTOs.Articles;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/articles")]
    public class ArticlesController(IArticleService articleService) : ControllerBase
    {
        private readonly IArticleService _articleService = articleService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ArticleDto>>> GetAll()
        {
            IEnumerable<ArticleDto> articles = await _articleService.GetAllPublishedAsync();
            return Ok(articles);
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<ArticleDto>> GetBySlug(string slug)
        {
            ArticleDto? article = await _articleService.GetBySlugAsync(slug);
            if (article == null)
                return NotFound();
            return Ok(article);
        }

        [HttpPost]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult<ArticleDto>> Create(CreateArticleDto dto)
        {
            string? authorIdStr = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (authorIdStr == null || !Guid.TryParse(authorIdStr, out Guid authorId))
                return Unauthorized();

            ArticleDto created = await _articleService.CreateAsync(dto, authorId);
            return CreatedAtAction(nameof(GetBySlug), new { slug = created.Slug }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult<ArticleDto>> Update(Guid id, UpdateArticleDto dto)
        {
            ArticleDto? updated = await _articleService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "BureauOrAdmin")]
        public async Task<ActionResult> Delete(Guid id)
        {
            bool deleted = await _articleService.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return Ok();
        }
    }
}
