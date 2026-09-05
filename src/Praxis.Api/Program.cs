using Praxis.Api.Configuration;
using Praxis.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddAi(builder.Configuration)
    .AddObjectStorage(builder.Configuration)
    .AddModules()
    .AddFrontendCors(builder.Configuration)
    .AddApiDocumentation();

var app = builder.Build();

// Swagger stays on in production too: this is a prototype, and whoever wires
// the frontend needs to see the contract without running anything locally.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Praxis API v1");
    options.DocumentTitle = "Praxis API";
    options.RoutePrefix = "swagger";
});

app.UseCors(CorsConfiguration.PolicyName);

app.MapHealthEndpoints();
app.MapUserEndpoints();
app.MapKnowledgeEndpoints();
app.MapVideoEndpoints();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

/// <summary>Exposed so integration tests can boot the API in memory.</summary>
public partial class Program;
