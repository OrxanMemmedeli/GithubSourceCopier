using GithubSourceCopier.MapService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace GithubSourceCopier.MapService.Endpoints;

public static class CityEndpoints
{
    public static void MapCityEndpoints(this WebApplication app)
    {
        app.MapGet("/city/{name}", async (
            [FromRoute, SwaggerParameter("Axtarılacaq şəhərin adı, məsələn, Oğuz", Required = true)] string name,
            IGraphService service) =>
        {
            try
            {
                var city = await service.GetCityAsync(name);
                return city != null ? Results.Ok(city) : Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "Şəhər haqqında məlumat",
            description: "Verilmiş şəhər adı əsasında şəhər haqqında məlumatı qaytarır."
        ));



        app.MapGet("/cities/{filter}", async (
            [FromRoute, SwaggerParameter("Şəhər adı üçün filtr, məsələn, 'Oğuz, Padar'", Required = true)] string filter,
            IGraphService service) =>
        {
            var cities = await service.SearchCitiesAsync(filter);
            return Results.Ok(cities);
        })
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "Şəhərlər siyahısı",
            description: "Verilmiş filtrə uyğun şəhərlər siyahısını qaytarır. Filtr şəhərin adında uyğunluğu yoxlayır."
        ));
    }
}
