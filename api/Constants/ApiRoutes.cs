namespace CouncilAgendaApi.Constants;

public static class ApiRoutes
{
    private const string Base = "api/v{version:apiVersion}";
    public const string OrgBase = Base + "/organizations/{orgId}";
    public const string Members = OrgBase + "/members";
    public const string Meetings = OrgBase + "/meetings";
    public const string Handbook = OrgBase + "/handbook";
    public const string MinistryAreas = OrgBase + "/ministry-areas";
	public const string Assignments = OrgBase + "/assignments";
    public const string Organizations = Base + "/organizations";
}