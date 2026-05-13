using CouncilAgendaApi.DTOs;

namespace CouncilAgendaApi.Services.Interfaces;

public interface IMeetingService
{
    Task<List<MeetingResponse>> GetMeetings(Guid orgId);
    Task<MeetingDetailResponse?> GetMeeting(Guid orgId, Guid meetingId);
    Task<MeetingResponse> CreateMeeting(Guid orgId, CreateMeetingRequest request);
    Task<bool> DeleteMeeting(Guid orgId, Guid meetingId);
}