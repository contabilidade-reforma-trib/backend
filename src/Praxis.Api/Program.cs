using Praxis.Api.Configuracao;
using Praxis.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AdicionarPersistencia(builder.Configuration)
    .AdicionarArmazenamentoDeObjetos(builder.Configuration)
    .AdicionarModulos()
    .AdicionarCorsDoFrontend(builder.Configuration)
    .AdicionarDocumentacao();

var app = builder.Build();

// Swagger fica ligado também em produção: é uma POC, e a pessoa que está
// integrando o front precisa enxergar o contrato sem subir nada localmente.
app.UseSwagger();
app.UseSwaggerUI(opcoes =>
{
    opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "Praxis API v1");
    opcoes.DocumentTitle = "Praxis API";
    opcoes.RoutePrefix = "swagger";
});

app.UseCors(ConfiguracaoDeCors.NomeDaPolitica);

app.MapEndpointsDeSaude();

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.Run();

/// <summary>Exposto para que os testes de integração possam subir a API em memória.</summary>
public partial class Program;
