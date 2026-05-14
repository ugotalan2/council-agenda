using Asp.Versioning;
using CouncilAgendaApi.Constants;
using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[ApiVersion(1)]
[Route(ApiRoutes.MinistryAreas)]
[Authorize]
public class MinistryAreasController : BaseController
{
    private readonly IMinistryAreaService _ministryAreaService;

    public MinistryAreasController(AppDbContext db, IMinistryAreaService ministryAreaService) : base(db)
    {
        _ministryAreaService = ministryAreaService;
    }

    [HttpGet]
    [Authorize(Policy = "OrgViewer")]
    public async Task<IActionResult> GetAreas(Guid orgId)
    {
        var areas = await _ministryAreaService.GetAreas(orgId);
        return Ok(areas);
    }

    [HttpPost]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> AddArea(Guid orgId, [FromBody] MinistryAreaRequest request)
    {
        var area = await _ministryAreaService.AddArea(orgId, request);
        return CreatedAtAction(nameof(GetAreas), new { orgId }, area);
    }

    [HttpPut("{areaId}")]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> UpdateArea(Guid orgId, Guid areaId, [FromBody] MinistryAreaRequest request)
    {
        var area = await _ministryAreaService.UpdateArea(orgId, areaId, request);
        if (area == null) return NotFound();
        return Ok(area);
    }

    [HttpDelete("{areaId}")]
    [Authorize(Policy = "OrgAdmin")]
    public async Task<IActionResult> DeleteArea(Guid orgId, Guid areaId)
    {
        var deleted = await _ministryAreaService.DeleteArea(orgId, areaId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
