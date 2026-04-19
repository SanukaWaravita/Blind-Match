using BlindMatchPAS.Models;
using BlindMatchPAS.Models.Enums;
using BlindMatchPAS.Services;
using BlindMatchPAS.Tests.Unit.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BlindMatchPAS.Tests.Unit.Services;

public class MatchingServiceTests
{
    [Fact]
    public async Task CannotRevealIdentityBeforeConfirmation()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext(nameof(CannotRevealIdentityBeforeConfirmation));
        var proposal = await SeedScenarioAsync(context);
        var audit = new Mock<BlindMatchPAS.Interfaces.IAuditService>();
        var service = new MatchingService(context, audit.Object);

        await service.ExpressInterestAsync(proposal.Id, "supervisor-user");
        var activeMatch = await service.GetActiveMatchByProposalIdAsync(proposal.Id);

        activeMatch.Should().BeNull();
        proposal.IsMatched.Should().BeFalse();
        context.MatchRecords.Should().BeEmpty();
    }

    [Fact]
    public async Task ConfirmMatchUpdatesStatusCorrectly()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext(nameof(ConfirmMatchUpdatesStatusCorrectly));
        var proposal = await SeedScenarioAsync(context);
        var audit = new Mock<BlindMatchPAS.Interfaces.IAuditService>();
        var service = new MatchingService(context, audit.Object);

        await service.ExpressInterestAsync(proposal.Id, "supervisor-user");
        await service.ConfirmMatchAsync(proposal.Id, "supervisor-user", "Ready to supervise");

        var refreshed = await context.ProjectProposals.FindAsync(proposal.Id);
        refreshed!.Status.Should().Be(ProposalStatus.Matched);
        refreshed.IsMatched.Should().BeTrue();
        refreshed.MatchedAt.Should().NotBeNull();
        context.MatchRecords.Single().RevealedAt.Should().NotBeNull();
        audit.Verify(x => x.LogAsync("MatchConfirmed", nameof(ProjectProposal), proposal.Id.ToString(), It.IsAny<string?>(), "supervisor-user"), Times.Once);
    }

    [Fact]
    public async Task CannotMatchWithdrawnProposal()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext(nameof(CannotMatchWithdrawnProposal));
        var proposal = await SeedScenarioAsync(context, ProposalStatus.Withdrawn, isWithdrawn: true);
        var audit = new Mock<BlindMatchPAS.Interfaces.IAuditService>();
        var service = new MatchingService(context, audit.Object);

        Func<Task> action = () => service.ConfirmMatchAsync(proposal.Id, "supervisor-user");
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Withdrawn proposals cannot be matched.*");
    }

    [Fact]
    public async Task CannotMatchAlreadyMatchedProposal()
    {
        using var context = TestDbContextFactory.CreateInMemoryContext(nameof(CannotMatchAlreadyMatchedProposal));
        var proposal = await SeedScenarioAsync(context, ProposalStatus.Matched, isMatched: true);
        var audit = new Mock<BlindMatchPAS.Interfaces.IAuditService>();
        var service = new MatchingService(context, audit.Object);

        Func<Task> action = () => service.ConfirmMatchAsync(proposal.Id, "supervisor-user");
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already matched*");
    }

    private static async Task<ProjectProposal> SeedScenarioAsync(BlindMatchPAS.Data.ApplicationDbContext context, ProposalStatus status = ProposalStatus.Pending, bool isWithdrawn = false, bool isMatched = false)
    {
        var studentUser = new ApplicationUser { Id = "student-user", UserName = "student@test.com", Email = "student@test.com", FullName = "Student One" };
        var supervisorUser = new ApplicationUser { Id = "supervisor-user", UserName = "supervisor@test.com", Email = "supervisor@test.com", FullName = "Supervisor One" };
        var area = new ResearchArea { Id = 1, Name = "AI" };
        var student = new StudentProfile { Id = 10, UserId = studentUser.Id, User = studentUser, StudentIdNumber = "ST12345", Department = "Computing", ContactNumber = "+94111234567" };
        var supervisor = new SupervisorProfile { Id = 20, UserId = supervisorUser.Id, User = supervisorUser, EmployeeId = "EMP123", Department = "Computing", ContactNumber = "+94117654321" };
        supervisor.ResearchAreas.Add(new SupervisorResearchArea { SupervisorProfileId = supervisor.Id, ResearchAreaId = area.Id, SupervisorProfile = supervisor, ResearchArea = area });
        var proposal = new ProjectProposal
        {
            Id = 30,
            Title = "AI Based Attendance",
            Abstract = new string('A', 120),
            TechnicalStack = "ASP.NET Core, SQL Server, ML.NET",
            ResearchArea = area,
            ResearchAreaId = area.Id,
            StudentProfile = student,
            StudentProfileId = student.Id,
            Status = status,
            IsWithdrawn = isWithdrawn,
            IsMatched = isMatched
        };

        context.AddRange(studentUser, supervisorUser, area, student, supervisor, proposal);
        await context.SaveChangesAsync();
        return proposal;
    }
}
