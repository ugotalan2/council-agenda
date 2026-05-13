using CouncilAgendaApi.Data;
using CouncilAgendaApi.DTOs;
using CouncilAgendaApi.Models;
using CouncilAgendaApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Services;

public class MemberService : IMemberService
{
    private readonly AppDbContext _db;

    public MemberService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<MemberResponse>> GetMembers(Guid orgId)
    {
        return await _db.Members
            .Where(m => m.OrganizationId == orgId && m.Active)
            .OrderBy(m => m.Name)
            .Select(m => new MemberResponse(
                m.Id,
                m.Name,
                m.Calling,
                m.Active,
                m.ClerkUserId
            ))
            .ToListAsync();
    }

    public async Task<MemberResponse> AddMember(Guid orgId, MemberRequest request)
    {
        var member = new Member
        {
            OrganizationId = orgId,
            Name = request.Name,
            Calling = request.Calling,
            ClerkUserId = request.ClerkUserId ?? string.Empty,
            Active = true
        };

        _db.Members.Add(member);
        await _db.SaveChangesAsync();

        return new MemberResponse(
            member.Id,
            member.Name,
            member.Calling,
            member.Active,
            member.ClerkUserId
        );
    }

    public async Task<MemberResponse?> UpdateMember(Guid orgId, Guid memberId, MemberRequest request)
    {
        var member = await _db.Members
            .FirstOrDefaultAsync(m => m.Id == memberId && m.OrganizationId == orgId);

        if (member == null) return null;

        member.Name = request.Name;
        member.Calling = request.Calling;
        if (request.ClerkUserId != null)
            member.ClerkUserId = request.ClerkUserId;

        await _db.SaveChangesAsync();

        return new MemberResponse(
            member.Id,
            member.Name,
            member.Calling,
            member.Active,
            member.ClerkUserId
        );
    }

    public async Task<bool> DeactivateMember(Guid orgId, Guid memberId)
    {
        var member = await _db.Members
            .FirstOrDefaultAsync(m => m.Id == memberId && m.OrganizationId == orgId);

        if (member == null) return false;

        member.Active = false;
        await _db.SaveChangesAsync();
        return true;
    }
}