namespace CouncilAgendaApi.Data;

public static class DefaultPositions
{
    public static readonly Dictionary<string, List<(string Title, bool IsStanding, bool IsGuestDefault, bool IsRotationEligible, int Order)>> ByOrgType = new()
    {
        ["bishopric"] = new()
        {
            ("Bishop", true, false, true, 1),
            ("1st Counselor", true, false, true, 2),
            ("2nd Counselor", true, false, true, 3),
            ("Executive Secretary", true, false, true, 4),
            ("Clerk", true, false, true, 5),
        },
        ["ward_council"] = new()
        {
            ("Bishop", true, false, true, 1),
            ("1st Counselor", true, false, true, 2),
            ("2nd Counselor", true, false, true, 3),
            ("Executive Secretary", true, false, true, 4),
            ("Elders Quorum President", true, false, true, 5),
            ("Relief Society President", true, false, true, 6),
            ("Sunday School President", true, false, true, 7),
            ("Primary President", true, false, true, 8),
            ("Young Women President", true, false, true, 9),
            ("High Councilor Representative", true, false, true, 10),
            // Guest defaults
            ("Full-time Missionaries", false, true, false, 11),
            ("Ward Mission Leader", false, true, false, 12),
            ("Temple & Family History Leader", false, true, false, 13),
            ("Self Reliance Specialist", false, true, false, 14),
            ("Emergency Preparedness Specialist", false, true, false, 15),
            ("Activity Committee Chair", false, true, false, 16),
        },
        ["ward_youth_council"] = new()
        {
            ("Bishop", true, false, true, 1),
            ("1st Counselor", true, false, true, 2),
            ("2nd Counselor", true, false, true, 3),
            ("Young Women President", true, false, true, 4),
            ("Priests Quorum 1st Assistant", true, false, true, 5),
            ("Teachers Quorum President", true, false, true, 6),
            ("Deacons Quorum President", true, false, true, 7),
            ("Gatherers of Light President", true, false, true, 8),
            ("Messengers of Hope President", true, false, true, 9),
            ("Builders of Faith President", true, false, true, 10),
        },
    };
}