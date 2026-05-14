using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Models;
using CouncilAgendaApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Services;

public class HandbookService : IHandbookService
{
    private readonly AppDbContext _db;

    public HandbookService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<HandbookSectionResponse>> GetSections(Guid orgId)
    {
        return await _db.HandbookSections
            .Where(h => h.OrganizationId == orgId)
            .OrderBy(h => h.Chapter)
            .ThenBy(h => h.Section)
            .Select(h => new HandbookSectionResponse(
                h.Id,
                h.Chapter,
                h.Section,
                h.Title,
                h.LastUsed,
                h.MinistryAreaId
            ))
            .ToListAsync();
    }

    public async Task<HandbookSectionResponse> AddSection(Guid orgId, HandbookSectionRequest request)
    {
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

        return new HandbookSectionResponse(
            section.Id,
            section.Chapter,
            section.Section,
            section.Title,
            section.LastUsed,
            section.MinistryAreaId
        );
    }
}
