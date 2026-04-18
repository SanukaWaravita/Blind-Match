namespace BlindMatchPAS.ViewModels.Admin;

public class StatusSummaryViewModel
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AreaSummaryViewModel
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DepartmentSummaryViewModel
{
    public string Department { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RecentMatchViewModel
{
    public string ProposalTitle { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string SupervisorName { get; set; } = string.Empty;
    public DateTime MatchedAt { get; set; }
}

public class RecentActivityViewModel
{
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
