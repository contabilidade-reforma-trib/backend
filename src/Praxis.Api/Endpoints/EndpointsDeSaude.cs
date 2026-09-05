using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Praxis.Modules.Persistencia;
using Praxis.Shared.Persistencia;

namespace Praxis.Api.Endpoints;

/// <summary>
/// Healthcheck que o front chama para provar que os dois ambientes deployados
/// se enxergam. Responde 200 mesmo com o banco fora do ar: quem chama precisa
/// distinguir "a API não respondeu" de "a API respondeu e o banco caiu".
/// </summary>
public static class EndpointsDeSaude
{
    public static void MapEndpointsDeSaude(this IEndpointRouteBuilder rotas)
    {
        var grupo = rotas.MapGroup("/api/saude").WithTags("Saúde");

        grupo.MapGet("/", ObterSaude)
            .WithName("ObterSaude")
            .WithSummary("Estado da API, do banco e do armazenamento")
            .Produces<RespostaDeSaude>();

        grupo.MapGet("/ping", () => Results.Ok(new { mensagem = "pong" }))
            .WithName("Ping")
            .WithSummary("Verificação mínima, sem tocar em banco");
    }

    private static async Task<IResult> ObterSaude(
        PraxisDbContext contexto,
        IOptions<OpcoesDeArmazenamento> opcoesDeArmazenamento,
        IWebHostEnvironment ambiente,
        CancellationToken cancellationToken)
    {
        var cronometro = Stopwatch.StartNew();
        bool bancoConectado;
        string? erroDoBanco = null;

        try
        {
            // Abrir a conexão de verdade, em vez de CanConnectAsync: aquele
            // devolve false engolindo a exceção, e o motivo — senha errada, host
            // inexistente, SSL recusado — é justamente o que se precisa saber
            // quando o banco não responde em produção.
            await contexto.Database.OpenConnectionAsync(cancellationToken);
            await contexto.Database.CloseConnectionAsync();
            bancoConectado = true;
        }
        catch (Exception excecao)
        {
            bancoConectado = false;

            // Só a mensagem, nunca a string de conexão: ela carrega a senha.
            erroDoBanco = excecao.GetBaseException().Message;
        }

        cronometro.Stop();

        var resposta = new RespostaDeSaude(
            Status: bancoConectado ? "ok" : "degradado",
            Servico: "praxis-api",
            Versao: ObterVersao(),
            Ambiente: ambiente.EnvironmentName,
            MomentoUtc: DateTimeOffset.UtcNow,
            Banco: new EstadoDoBanco(bancoConectado, cronometro.ElapsedMilliseconds, erroDoBanco),
            Armazenamento: new EstadoDoArmazenamento(opcoesDeArmazenamento.Value.EstaConfigurado));

        return Results.Ok(resposta);
    }

    private static string ObterVersao() =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
}

public sealed record RespostaDeSaude(
    string Status,
    string Servico,
    string Versao,
    string Ambiente,
    DateTimeOffset MomentoUtc,
    EstadoDoBanco Banco,
    EstadoDoArmazenamento Armazenamento);

public sealed record EstadoDoBanco(bool Conectado, long LatenciaMs, string? Erro);

public sealed record EstadoDoArmazenamento(bool Configurado);
