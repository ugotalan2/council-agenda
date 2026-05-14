using CouncilAgendaApi.DTOs;

namespace CouncilAgendaApi.Services.Interfaces;

public interface IMinistryAreaService
{
    Task<List<MinistryAreaResponse>> GetAreas(Guid orgId);
    Task<MinistryAreaResponse> AddArea(Guid orgId, MinistryAreaRequest request);
    Task<MinistryAreaResponse?> UpdateArea(Guid orgId, Guid areaId, MinistryAreaRequest request);
    Task<bool> DeleteArea(Guid orgId, Guid areaId);
}
