using BlindMatchPAS.Models;
using BlindMatchPAS.ViewModels.Proposal;

namespace BlindMatchPAS.Interfaces;

public interface IProposalService
{
    Task<int> CreateAsync(string studentUserId, ProposalFormViewModel model);
    Task UpdateAsync(int proposalId, string studentUserId, ProposalFormViewModel model);
    Task WithdrawAsync(int proposalId, string studentUserId);
    Task<ProjectProposal?> GetByIdAsync(int id);
    Task<ProjectProposal?> GetByIdForStudentAsync(int id, string studentUserId);
    Task<List<ProjectProposal>> GetStudentProposalsAsync(string studentUserId);
    Task<List<ProjectProposal>> GetAdminProposalsAsync(ProposalFilterViewModel filter);
    Task<List<ProjectProposal>> GetAnonymousProposalsForSupervisorAsync(string supervisorUserId, ProposalFilterViewModel filter);
    Task<bool> CanStudentModifyAsync(int proposalId, string studentUserId);
}
