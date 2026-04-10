using BlindMatchPAS.Models;

namespace BlindMatchPAS.Interfaces;

public interface IMatchingService
{
    Task ExpressInterestAsync(int proposalId, string supervisorUserId);
    Task ConfirmMatchAsync(int proposalId, string supervisorUserId, string? notes = null);
    Task ManualAssignAsync(int proposalId, int supervisorProfileId, string adminUserId, string? notes = null);
    Task<List<InterestRecord>> GetSupervisorInterestsAsync(string supervisorUserId);
    Task<List<MatchRecord>> GetSupervisorMatchesAsync(string supervisorUserId);
    Task<MatchRecord?> GetActiveMatchByProposalIdAsync(int proposalId);
}
