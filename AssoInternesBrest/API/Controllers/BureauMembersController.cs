using AssoInternesBrest.API.DTOs.BureauMembers;
using AssoInternesBrest.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssoInternesBrest.API.Controllers
{
    [ApiController]
    [Route("api/bureau")]
    public class BureauMembersController(IBureauMemberService bureauMemberService) : ControllerBase
    {
        private readonly IBureauMemberService _bureauMemberService = bureauMemberService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BureauMemberDto>>> GetAll()
        {
            IEnumerable<BureauMemberDto> members = await _bureauMemberService.GetAllAsync();
            return Ok(members);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<BureauMemberDto>> Create(CreateBureauMemberDto dto)
        {
            BureauMemberDto created = await _bureauMemberService.CreateAsync(dto);
            return StatusCode(201, created);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<BureauMemberDto>> Update(Guid id, UpdateBureauMemberDto dto)
        {
            BureauMemberDto? updated = await _bureauMemberService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> Delete(Guid id)
        {
            bool deleted = await _bureauMemberService.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return Ok();
        }
    }
}
