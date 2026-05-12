using CouncilAgendaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CouncilAgendaApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<UserOrganization> UserOrganizations => Set<UserOrganization>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<AgendaItem> AgendaItems => Set<AgendaItem>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<RotationLog> RotationLogs => Set<RotationLog>();
    public DbSet<MinistryArea> MinistryAreas => Set<MinistryArea>();
    public DbSet<HandbookSection> HandbookSections => Set<HandbookSection>();
    public DbSet<TopicBacklogItem> TopicBacklogItems => Set<TopicBacklogItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Organization>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(o => o.CreatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Member>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(m => m.Organization)
             .WithMany(o => o.Members)
             .HasForeignKey(m => m.OrganizationId);
        });

        modelBuilder.Entity<UserOrganization>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(u => u.JoinedAt).HasDefaultValueSql("now()");
            e.HasOne(u => u.Organization)
             .WithMany(o => o.UserOrganizations)
             .HasForeignKey(u => u.OrganizationId);
        });

        modelBuilder.Entity<Meeting>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(m => m.CreatedAt).HasDefaultValueSql("now()");
            e.HasOne(m => m.Organization)
             .WithMany(o => o.Meetings)
             .HasForeignKey(m => m.OrganizationId);
        });

        modelBuilder.Entity<AgendaItem>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(a => a.Meeting)
             .WithMany(m => m.AgendaItems)
             .HasForeignKey(a => a.MeetingId);
            e.HasOne(a => a.HandbookSection)
             .WithMany()
             .HasForeignKey(a => a.HandbookSectionId);
            e.HasOne(a => a.TopicBacklogItem)
             .WithMany()
             .HasForeignKey(a => a.TopicBacklogId);
        });

        modelBuilder.Entity<Assignment>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(a => a.CreatedAt).HasDefaultValueSql("now()");
            e.HasOne(a => a.Meeting)
             .WithMany(m => m.Assignments)
             .HasForeignKey(a => a.MeetingId);
            e.HasOne(a => a.Owner)
             .WithMany(m => m.Assignments)
             .HasForeignKey(a => a.OwnerId);
        });

        modelBuilder.Entity<RotationLog>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(r => r.Member)
             .WithMany(m => m.RotationLogs)
             .HasForeignKey(r => r.MemberId);
        });

        modelBuilder.Entity<MinistryArea>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<HandbookSection>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(h => h.MinistryArea)
             .WithMany(m => m.HandbookSections)
             .HasForeignKey(h => h.MinistryAreaId);
        });

        modelBuilder.Entity<TopicBacklogItem>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(t => t.CreatedAt).HasDefaultValueSql("now()");
            e.HasOne(t => t.MinistryArea)
             .WithMany(m => m.TopicBacklogItems)
             .HasForeignKey(t => t.MinistryAreaId);
        });
    }
}