namespace CouncilAgendaApi.Models;

public class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OrgType { get; set; } = string.Empty;
    public bool ConductingRotates { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();
    public ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
    public ICollection<Member> Members { get; set; } = new List<Member>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<RecurringResponsibility> RecurringResponsibilities { get; set; } = new List<RecurringResponsibility>();
}