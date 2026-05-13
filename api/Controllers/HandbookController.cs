using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Services;
using CouncilAgendaApi.Services.Interfaces;
using CouncilAgendaApi.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace CouncilAgendaApi.Controllers;

[ApiController]
[ApiVersion(1)]
[Route(ApiRoutes.Handbook)]
[Authorize]
public class HandbookController : BaseController
{
	private readonly IHandbookService _handbookService;
    private readonly DiscussionQuestionService _questionService;

    public HandbookController(
		AppDbContext db, 
		IHandbookService handbookService,
		DiscussionQuestionService questionService) : base(db)
    {
        _handbookService = handbookService;
        _questionService = questionService;
    }

    [HttpGet]
    [Authorize(Policy = "OrgViewer")]
    public async Task<IActionResult> GetSections(Guid orgId)
    {
        var sections = await _handbookService.GetSections(orgId);
        return Ok(sections);
    }

    [HttpPost]
	[Authorize(Policy = "OrgEditor")]
    public async Task<IActionResult> AddSection(Guid orgId, [FromBody] HandbookSectionRequest request)
    {
        var section = await _handbookService.AddSection(orgId, request);
        return CreatedAtAction(nameof(GetSections), new { orgId }, section);
    }

    [HttpGet("{sectionId}/questions")]
    [Authorize(Policy = "OrgViewer")]
    public async Task<IActionResult> GetDiscussionQuestions(Guid orgId, Guid sectionId)
    {
        var questions = await _questionService.GenerateQuestions(sectionId);
        return Ok(new DiscussionQuestionsResponse(sectionId, "", questions));
    }
}