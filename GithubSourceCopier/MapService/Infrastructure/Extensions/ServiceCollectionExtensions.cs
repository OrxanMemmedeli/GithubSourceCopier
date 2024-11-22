using GithubSourceCopier.MapService.Core.Interfaces;
using GithubSourceCopier.MapService.Core.Services;
using GithubSourceCopier.MapService.Infrastructure.Configurations;
using Neo4j.Driver;

namespace GithubSourceCopier.MapService.Infrastructure.Extensions;


public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGraphServices(this IServiceCollection services)
    {

        // GraphService üçün HttpClient əlavə edilir
        services.AddHttpClient<IGraphService, GraphService>();


        //services.AddHttpClient<IPathFindingService, PathFindingService>()
        //          .AddTypedClient(httpClient => new PathFindingService(httpClient, googleMapsApiKey));


        services.AddHttpClient<IPathFindingService, PathFindingService>();


        return services;
    }
}
