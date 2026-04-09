using BlindMatchPAS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<SupervisorProfile> SupervisorProfiles => Set<SupervisorProfile>();
    public DbSet<ResearchArea> ResearchAreas => Set<ResearchArea>();
    public DbSet<SupervisorResearchArea> SupervisorResearchAreas => Set<SupervisorResearchArea>();
    public DbSet<ProjectProposal> ProjectProposals => Set<ProjectProposal>();
    public DbSet<MatchRecord> MatchRecords => Set<MatchRecord>();
    public DbSet<InterestRecord> InterestRecords => Set<InterestRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().HasIndex(u => u.Email).IsUnique();
        builder.Entity<StudentProfile>().HasIndex(s => s.UserId).IsUnique();
        builder.Entity<StudentProfile>().HasIndex(s => s.StudentIdNumber).IsUnique();
        builder.Entity<SupervisorProfile>().HasIndex(s => s.UserId).IsUnique();
        builder.Entity<SupervisorProfile>().HasIndex(s => s.EmployeeId).IsUnique();
        builder.Entity<ResearchArea>().HasIndex(r => r.Name).IsUnique();

        builder.Entity<SupervisorResearchArea>().HasKey(x => new { x.SupervisorProfileId, x.ResearchAreaId });

        builder.Entity<SupervisorResearchArea>()
            .HasOne(x => x.SupervisorProfile)
            .WithMany(x => x.ResearchAreas)
            .HasForeignKey(x => x.SupervisorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SupervisorResearchArea>()
            .HasOne(x => x.ResearchArea)
            .WithMany(x => x.SupervisorResearchAreas)
            .HasForeignKey(x => x.ResearchAreaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProjectProposal>()
            .HasOne(p => p.StudentProfile)
            .WithMany(s => s.Proposals)
            .HasForeignKey(p => p.StudentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProjectProposal>()
            .HasOne(p => p.ResearchArea)
            .WithMany(r => r.Proposals)
            .HasForeignKey(p => p.ResearchAreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InterestRecord>().HasIndex(i => new { i.ProposalId, i.SupervisorProfileId }).IsUnique();

        builder.Entity<MatchRecord>()
            .HasOne(m => m.Proposal)
            .WithMany(p => p.MatchRecords)
            .HasForeignKey(m => m.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MatchRecord>()
            .HasOne(m => m.SupervisorProfile)
            .WithMany(s => s.MatchRecords)
            .HasForeignKey(m => m.SupervisorProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
