using GithubSourceCopier.MapService.Core.Interfaces;
using GithubSourceCopier.MapService.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GithubSourceCopier.MapService.Endpoints;


public static class PathEndpoints
{
    public static void MapPathEndpoints(this WebApplication app)
    {

        app.MapGet("/path/shortest", async (
            [FromQuery, SwaggerParameter("Başlanğıc nöqtənin koordinatları, məsələn, 49.8671,40.4093", Required = true)] string originCordinates,
            [FromQuery, SwaggerParameter("Təyinat nöqtənin koordinatları, məsələn, 46.3606,40.6828", Required = true)] string destinationCordinates,
            IPathFindingService service) =>
        {
            try
            {
                var path = await service.FindShortestPathAsync(originCordinates, destinationCordinates);
                return path != null ? Results.Ok(path) : Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "Ən qısa yolu tap",
            description: "Başlanğıc və təyinat koordinatları əsasında ən qısa yolu tapır. Koordinatlar `latitude,longitude` formatında verilməlidir, məsələn, `49.8671,40.4093`."
        ));

        app.MapGet("/path/detailed-text", async (
            [FromQuery, SwaggerParameter("Başlanğıc nöqtənin təsviri, məsələn, Bakı, Yasamal, 20 yanvar metrosu", Required = true)] string origin,
            [FromQuery, SwaggerParameter("Təyinat nöqtənin təsviri, məsələn, Oğuz rayonu Padar kəndi", Required = true)] string destination,
            IPathFindingService service) =>
        {
            try
            {
                var routeDetails = await service.FindDetailedRouteAsync(origin, destination);
                return Results.Ok(routeDetails);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "Marşrut detallarını text olaraq qaytarır",
            description: "Başlanğıc və təyinat nöqtələri arasında marşrutun addım-addım təsvirini qaytarır."
        ));



        app.MapGet("/path/detailed-map", async (
            [FromQuery, SwaggerParameter("Başlanğıc nöqtənin təsviri, məsələn, Bakı, Yasamal, 20 yanvar metrosu", Required = true)] string origin,
            [FromQuery, SwaggerParameter("Təyinat nöqtənin təsviri, məsələn, Oğuz rayonu Padar kəndi", Required = true)] string destination,
            IPathFindingService service) =>
        {
            try
            {
                var mapData = await service.FindRouteForMapAsync(origin, destination);
                return Results.Ok(mapData);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "HTML xəritəsi üçün məlumatları qaytarır",
            description: "Başlanğıc və təyinat nöqtələri arasında xəritədə göstəriləcək marşrut məlumatlarını qaytarır."
        ));


    }
}