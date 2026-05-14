using CouncilAgendaApi.DTOs;

namespace CouncilAgendaApi.Services.Interfaces;

public interface IHandbookService
{
    Task<List<HandbookSectionResponse>> GetSections(Guid orgId);
    Task<HandbookSectionResponse> AddSection(Guid orgId, HandbookSectionRequest request);
}
