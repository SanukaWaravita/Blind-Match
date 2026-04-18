using BlindMatchPAS.Models.Enums;

namespace BlindMatchPAS.Helpers;

public static class StatusBadgeHelper
{
    public static string GetBadgeClass(ProposalStatus status) => status switch
    {
        ProposalStatus.Pending => "badge-soft-warning",
        ProposalStatus.UnderReview => "badge-soft-info",
        ProposalStatus.Interested => "badge-soft-primary",
        ProposalStatus.Matched => "badge-soft-success",
        ProposalStatus.Withdrawn => "badge-soft-danger",
        ProposalStatus.Reassigned => "badge-soft-dark",
        _ => "badge bg-secondary"
    };
}
