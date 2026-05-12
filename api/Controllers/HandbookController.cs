using CouncilAgendaApi.Data;
using CouncilAgendaApi.Models;
using CouncilAgendaApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[Route("api/organizations/{orgId}/handbook")]
[Authorize]
public class HandbookController : BaseController
{
    private readonly DiscussionQuestionService _questionService;

    public HandbookController(AppDbContext db, DiscussionQuestionService questionService) : base(db)
    {
        _questionService = questionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSections(Guid orgId)
    {
        if (!await HasAccess(orgId)) return Forbid();

        var sections = await _db.HandbookSections
            .Where(h => h.OrganizationId == orgId)
            .OrderBy(h => h.Chapter)
            .ThenBy(h => h.Section)
            .Select(h => new {
                h.Id,
                h.Chapter,
                h.Section,
                h.Title,
                h.LastUsed,
                h.MinistryAreaId
            })
            .ToListAsync();

        return Ok(sections);
    }

    [HttpPost]
    public async Task<IActionResult> AddSection(Guid orgId, [FromBody] HandbookSectionRequest request)
    {
        if (!await HasAccess(orgId, "editor")) return Forbid();

        var section = new HandbookSection
        {
            OrganizationId = orgId,
            Chapter = request.Chapter,
            Section = request.Section,
            Title = request.Title,
            Content = request.Content,
            MinistryAreaId = request.MinistryAreaId
        };

        _db.HandbookSections.Add(section);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSections), new { orgId }, section);
    }

    [HttpGet("{sectionId}/questions")]
    public async Task<IActionResult> GetDiscussionQuestions(Guid orgId, Guid sectionId)
    {
        if (!await HasAccess(orgId, "editor")) return Forbid();

        try
        {
            var questions = await _questionService.GenerateQuestions(sectionId);
            return Ok(questions);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public record HandbookSectionRequest(
    string Chapter,
    string Section,
    string Title,
    string Content,
    Guid? MinistryAreaId);