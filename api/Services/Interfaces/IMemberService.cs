using CouncilAgendaApi.DTOs;

namespace CouncilAgendaApi.Services.Interfaces;

public interface IMemberService
{
    Task<List<MemberResponse>> GetMembers(Guid orgId);
    Task<MemberResponse> AddMember(Guid orgId, MemberRequest request);
    Task<MemberResponse?> UpdateMember(Guid orgId, Guid memberId, MemberRequest request);
    Task<bool> DeactivateMember(Guid orgId, Guid memberId);
}
