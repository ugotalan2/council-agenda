namespace CouncilAgendaApi.Data;

public static class DefaultResponsibilities
{
    public static readonly List<(string Title, string? LcrUrl, int MinWeeks, int MaxWeeks, string OrgTypeScope)> Defaults = new()
    {
        // Bishopric only
        (
            "Temple Recommends",
            "https://lcr.churchofjesuschrist.org/records/member-list?type=RECOMMEND_EXPIRING",
            3, 4, "bishopric"
        ),
        (
            "Tithing Status",
            "https://lcr.churchofjesuschrist.org/donations/tithing-declaration",
            4, 6, "bishopric"
        ),
        (
            "Endowment Candidates",
            "https://lcr.churchofjesuschrist.org/records/member-list",
            4, 6, "bishopric"
        ),

        // Ward Council only
        (
            "Sacrament Meeting Attendance",
            "https://lcr.churchofjesuschrist.org/report/sacrament-attendance",
            2, 3, "ward_council"
        ),
        (
            "Child and Youth Protection Training",
            "https://lcr.churchofjesuschrist.org/report/training-and-certification",
            4, 6, "ward_council"
        ),
        (
            "Ministering Interviews",
            "https://lcr.churchofjesuschrist.org/report/ministering",
            4, 8, "ward_council"
        ),
        (
            "Member Moves In/Out",
            "https://lcr.churchofjesuschrist.org/records/member-list?type=RECENT_MOVE_IN",
            2, 3, "ward_council"
        ),
        (
            "New Members",
            "https://lcr.churchofjesuschrist.org/records/member-list?type=RECENT_CONVERT",
            2, 4, "ward_council"
        ),
        (
            "Less Active Members",
            "https://lcr.churchofjesuschrist.org/report/less-active-members",
            2, 4, "ward_council"
        ),
        (
            "Missionary Progress",
            "https://lcr.churchofjesuschrist.org/report/progress-record",
            2, 3, "ward_council"
        ),
    };
}