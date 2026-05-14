using CouncilAgendaApi.DTOs;

namespace CouncilAgendaApi.Services.Interfaces;

public interface IAssignmentService
{
    Task<List<AssignmentResponse>> GetAssignments(Guid orgId, string? status = null);
    Task<AssignmentResponse> CreateAssignment(Guid orgId, CreateAssignmentRequest request);
    Task<AssignmentResponse?> UpdateStatus(Guid orgId, Guid assignmentId, UpdateAssignmentStatusRequest request);
    Task<bool> DeleteAssignment(Guid orgId, Guid assignmentId);
}
