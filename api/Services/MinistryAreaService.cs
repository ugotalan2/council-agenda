using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Models;
using CouncilAgendaApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Services;

public class MinistryAreaService : IMinistryAreaService
{
    private readonly AppDbContext _db;

    public MinistryAreaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<MinistryAreaResponse>> GetAreas(Guid orgId)
    {
        return await _db.MinistryAreas
            .Where(m => m.OrganizationId == orgId)
            .OrderBy(m => m.Name)
            .Select(m => new MinistryAreaResponse(
                m.Id,
                m.Name,
                m.LastFocused
            ))
            .ToListAsync();
    }

    public async Task<MinistryAreaResponse> AddArea(Guid orgId, MinistryAreaRequest request)
    {
        var area = new MinistryArea
        {
            OrganizationId = orgId,
            Name = request.Name
        };

        _db.MinistryAreas.Add(area);
        await _db.SaveChangesAsync();

        return new MinistryAreaResponse(area.Id, area.Name, area.LastFocused);
    }

    public async Task<MinistryAreaResponse?> UpdateArea(Guid orgId, Guid areaId, MinistryAreaRequest request)
    {
        var area = await _db.MinistryAreas
            .FirstOrDefaultAsync(m => m.Id == areaId && m.OrganizationId == orgId);

        if (area == null) return null;

        area.Name = request.Name;
        await _db.SaveChangesAsync();

        return new MinistryAreaResponse(area.Id, area.Name, area.LastFocused);
    }

    public async Task<bool> DeleteArea(Guid orgId, Guid areaId)
    {
        var area = await _db.MinistryAreas
            .FirstOrDefaultAsync(m => m.Id == areaId && m.OrganizationId == orgId);

        if (area == null) return false;

        _db.MinistryAreas.Remove(area);
        await _db.SaveChangesAsync();
        return true;
    }
}
