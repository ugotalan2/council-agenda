using Asp.Versioning;
using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("api/[controller]")]
[Authorize]
public class OrganizationsController : BaseController
{
    private readonly IOrganizationService _orgService;

    public OrganizationsController(AppDbContext db, IOrganizationService orgService) : base(db)
    {
        _orgService = orgService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrganizations()
    {
        var orgs = await _orgService.GetUserOrganizations(GetClerkUserId());
        return Ok(orgs);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrgRequest request)
    {
        var org = await _orgService.CreateOrganization(GetClerkUserId(), request);
        return CreatedAtAction(nameof(GetMyOrganizations), new { id = org.Id }, org);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrganization(Guid id)
    {
        var deleted = await _orgService.DeleteOrganization(GetClerkUserId(), id);
        if (!deleted) return Forbid();
        return NoContent();
    }
}