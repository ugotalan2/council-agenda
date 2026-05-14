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
[Route(ApiRoutes.Assignments)]
[Authorize]
public class AssignmentsController : BaseController
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentsController(AppDbContext db, IAssignmentService assignmentService) : base(db)
    {
        _assignmentService = assignmentService;
    }

    [HttpGet]
    [Authorize(Policy = "OrgViewer")]
    public async Task<IActionResult> GetAssignments(Guid orgId, [FromQuery] string? status = null)
    {
        var assignments = await _assignmentService.GetAssignments(orgId, status);
        return Ok(assignments);
    }

    [HttpPost]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> CreateAssignment(Guid orgId, [FromBody] CreateAssignmentRequest request)
    {
        var assignment = await _assignmentService.CreateAssignment(orgId, request);
        return CreatedAtAction(nameof(GetAssignments), new { orgId }, assignment);
    }

    [HttpPut("{assignmentId}/status")]
    [Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> UpdateStatus(
        Guid orgId,
        Guid assignmentId,
        [FromBody] UpdateAssignmentStatusRequest request)
    {
        var assignment = await _assignmentService.UpdateStatus(orgId, assignmentId, request);
        if (assignment == null) return NotFound();
        return Ok(assignment);
    }

    [HttpDelete("{assignmentId}")]
    [Authorize(Policy = "OrgAdmin")]
    public async Task<IActionResult> DeleteAssignment(Guid orgId, Guid assignmentId)
    {
        var deleted = await _assignmentService.DeleteAssignment(orgId, assignmentId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
