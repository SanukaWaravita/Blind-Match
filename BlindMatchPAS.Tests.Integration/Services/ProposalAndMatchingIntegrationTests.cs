using BlindMatchPAS.Models;
using BlindMatchPAS.Models.Enums;
using BlindMatchPAS.Services;
using BlindMatchPAS.Tests.Integration.Data;
using BlindMatchPAS.ViewModels.Proposal;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BlindMatchPAS.Tests.Integration.Services;

public class ProposalAndMatchingIntegrationTests
{
    [Fact]
    public async Task ProposalCreationPersistsCorrectly()
    {
        using var factory = new SqliteTestDbFactory();
        await using var context = factory.CreateContext();
        var student = await SeedBaseScenarioAsync(context);
        var audit = new Mock<BlindMatchPAS.Interfaces.IAuditService>();
        var service = new ProposalService(context, audit.Object);

        var id = await service.CreateAsync(student.UserId, new ProposalFormViewModel
        {
            Title = "Secure E-Learning Platform",
            Abstract = new string('B', 120),
            TechnicalStack = "ASP.NET Core, SQL Server, Bootstrap",
            ResearchAreaId = 1,
            AdditionalDescription = "Detailed proposal notes"
        });

        var stored = await context.ProjectProposals.FindAsync(id);
        stored.Should().NotBeNull();
        stored!.Title.Should().Be("Secure E-Learning Platform");
        stored.ResearchAreaId.Should().Be(1);
    }

    [Fact]
    public async Task ResearchAreaLinkingWorks()
    {
        using var factory = new SqliteTestDbFactory();
        await using var context = factory.CreateContext();
        await SeedBaseScenarioAsync(context, includeSupervisor: true);

        var linked = await context.SupervisorResearchAreas
            .Where(x => x.SupervisorProfileId == 20)
            .ToListAsync();

        linked.Should().ContainSingle();
        linked.Single().ResearchAreaId.Should().Be(1);
    }

    [Fact]
    public async Task MatchCreationUpdatesDatabaseState()
    {
        using var factory = new SqliteTestDbFactory();
        await using var context = factory.CreateContext();
        await SeedProposalWithSupervisorAsync(context);
        var audit = new Mock<BlindMatchPAS.Interfaces.IAuditService>();
        var service = new MatchingService(context, audit.Object);

        await service.ExpressInterestAsync(30, "supervisor-user");
        await service.ConfirmMatchAsync(30, "supervisor-user");

        var proposal = await context.ProjectProposals.FindAsync(30);
        var match = await context.MatchRecords.SingleAsync();

        proposal!.Status.Should().Be(ProposalStatus.Matched);
        proposal.IsMatched.Should().BeTrue();
        match.IsActive.Should().BeTrue();
        match.RevealedAt.Should().NotBeNull();
    }

    private static async Task<StudentProfile> SeedBaseScenarioAsync(BlindMatchPAS.Data.ApplicationDbContext context, bool includeSupervisor = false)
    {
        var area = new ResearchArea { Id = 1, Name = "Software Engineering" };
        var studentUser = new ApplicationUser { Id = "student-user", UserName = "student@test.com", Email = "student@test.com", FullName = "Student One" };
        var student = new StudentProfile
        {
            Id = 10,
            UserId = studentUser.Id,
            User = studentUser,
            StudentIdNumber = "ST54321",
            Department = "Computing",
            ContactNumber = "+94123456789"
        };

        context.AddRange(area, studentUser, student);

        if (includeSupervisor)
        {
            var supervisorUser = new ApplicationUser { Id = "supervisor-user", UserName = "supervisor@test.com", Email = "supervisor@test.com", FullName = "Supervisor One" };
            var supervisor = new SupervisorProfile
            {
                Id = 20,
                UserId = supervisorUser.Id,
                User = supervisorUser,
                EmployeeId = "EMP456",
                Department = "Computing",
                ContactNumber = "+94999888777"
            };
            context.AddRange(supervisorUser, supervisor, new SupervisorResearchArea
            {
                SupervisorProfileId = supervisor.Id,
                SupervisorProfile = supervisor,
                ResearchAreaId = area.Id,
                ResearchArea = area
            });
        }

        await context.SaveChangesAsync();
        return includeSupervisor
            ? await context.StudentProfiles.FirstAsync()
            : student;
    }

    private static async Task SeedProposalWithSupervisorAsync(BlindMatchPAS.Data.ApplicationDbContext context)
    {
        var area = new ResearchArea { Id = 1, Name = "Software Engineering" };
        var studentUser = new ApplicationUser { Id = "student-user", UserName = "student@test.com", Email = "student@test.com", FullName = "Student One" };
        var supervisorUser = new ApplicationUser { Id = "supervisor-user", UserName = "supervisor@test.com", Email = "supervisor@test.com", FullName = "Supervisor One" };
        var student = new StudentProfile { Id = 10, UserId = studentUser.Id, User = studentUser, StudentIdNumber = "ST22222", Department = "Computing", ContactNumber = "+94123456000" };
        var supervisor = new SupervisorProfile { Id = 20, UserId = supervisorUser.Id, User = supervisorUser, EmployeeId = "EMP888", Department = "Computing", ContactNumber = "+94111111000" };
        var proposal = new ProjectProposal
        {
            Id = 30,
            Title = "Blind Allocation Platform",
            Abstract = new string('C', 150),
            TechnicalStack = "ASP.NET Core, EF Core, SQL Server",
            ResearchAreaId = 1,
            ResearchArea = area,
            StudentProfileId = 10,
            StudentProfile = student
        };

        context.AddRange(area, studentUser, supervisorUser, student, supervisor, proposal);
        context.Add(new SupervisorResearchArea
        {
            SupervisorProfileId = 20,
            SupervisorProfile = supervisor,
            ResearchAreaId = 1,
            ResearchArea = area
        });
        await context.SaveChangesAsync();
    }
}
