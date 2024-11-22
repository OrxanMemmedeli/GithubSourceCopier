using GithubSourceCopier.MapService.Core.Interfaces;
using GithubSourceCopier.MapService.Core.Services;
using GithubSourceCopier.MapService.Infrastructure.Configurations;
using Neo4j.Driver;

namespace GithubSourceCopier.MapService.Infrastructure.Extensions;


public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGraphServices(this IServiceCollection services, Neo4jConfig config)
    {
        services.AddSingleton(GraphDatabase.Driver(config.Uri, AuthTokens.Basic(config.Username, config.Password)));
        services.AddScoped<IGraphService, GraphService>();
        services.AddScoped<IPathFindingService, PathFindingService>();
        return services;
    }
}
