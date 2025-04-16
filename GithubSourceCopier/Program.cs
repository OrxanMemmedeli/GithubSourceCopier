using GithubSourceCopier.Endpoints;
using GithubSourceCopier.MapService.Endpoints;
using GithubSourceCopier.MapService.Infrastructure.Configurations;
using GithubSourceCopier.MapService.Infrastructure.Extensions;
using GithubSourceCopier.Models;
using GithubSourceCopier.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpClient();
builder.Services.AddScoped<IProjectUpdaterService, ProjectUpdaterService>();


builder.Services.AddGraphServices();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations(); // Anotasiya dəstəyi aktiv edilir
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.MapCityEndpoints();
app.MapPathEndpoints();
app.MapURLEndpoints();

app.MapPost("/api/project-updater/update-project", async (ProjectUpdateRequest request, IProjectUpdaterService projectUpdaterService) =>
{
    // Validate the request
    if (string.IsNullOrWhiteSpace(request.GitHubLink) || string.IsNullOrWhiteSpace(request.LocalPath) || string.IsNullOrWhiteSpace(request.TargetNamespace))
    {
        return Results.BadRequest("GitHub linki, lokal yol və namespace sahəsi tələb olunur.");
    }

    try
    {
        var addedFiles = new List<string>();

        // Process the request and retrieve the added files
        await foreach (var addedFile in projectUpdaterService.DownloadAndCopyFilesAsync(
                                request.GitHubLink,
                                request.LocalPath,
                                request.OldVersion,
                                request.NewVersion,
                                request.TargetNamespace))
        {
            addedFiles.Add(addedFile);
            Console.WriteLine($"\n\rƏlavə edilən fayl: {addedFile}");
        }

        return Results.Ok(new { Message = "Bütün fayllar uğurla əlavə edildi.", AddedFiles = addedFiles });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Xəta baş verdi: {ex.Message}");
    }
})
.WithName("UpdateProject");// Endpoint adı


app.Run();
