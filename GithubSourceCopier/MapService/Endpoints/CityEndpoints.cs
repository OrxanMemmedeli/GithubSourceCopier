using GithubSourceCopier.MapService.Core.Interfaces;
using GithubSourceCopier.MapService.Core.Models;

namespace GithubSourceCopier.MapService.Endpoints;

public static class CityEndpoints
{
    public static void MapCityEndpoints(this WebApplication app)
    {
        app.MapPost("/city", async (CityModel city, IGraphService service) =>
        {
            await service.AddCityAsync(city);
            return Results.Ok();
        });

        app.MapGet("/city/{filter}", async (string filter, IGraphService service) =>
        {
            var cities = await service.GetCitiesAsync(filter);
            return Results.Ok(cities);
        });
    }
}
