using BlindMatchPAS.Interfaces;
using BlindMatchPAS.Services;

namespace BlindMatchPAS.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlindMatchServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProposalService, ProposalService>();
        services.AddScoped<IMatchingService, MatchingService>();
        services.AddScoped<IResearchAreaService, ResearchAreaService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddHttpContextAccessor();
        return services;
    }
}
