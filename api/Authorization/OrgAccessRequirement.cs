using Microsoft.AspNetCore.Authorization;

namespace CouncilAgendaApi.Authorization;

public class OrgAccessRequirement : IAuthorizationRequirement
{
    public string MinRole { get; }
    public OrgAccessRequirement(string minRole = "viewer")
    {
        MinRole = minRole;
    }
}