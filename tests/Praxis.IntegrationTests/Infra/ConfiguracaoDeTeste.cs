using System.Text.Json;

namespace Praxis.IntegrationTests.Infra;

/// <summary>
/// Descobre a string de conexão dos testes de integração sem exigir configuração
/// extra na máquina de quem clonou. Se não encontrar nenhuma, os testes de
/// integração são pulados em vez de falharem — assim <c>dotnet test</c> continua
/// verde para quem ainda não recebeu as credenciais, e o portão de push não
/// trava por um motivo que não é culpa de quem está desenvolvendo.
/// </summary>
public static class ConfiguracaoDeTeste
{
    private static readonly Lazy<string?> conexao = new(Descobrir);

    public static string? StringDeConexao => conexao.Value;

    public static bool EstaConfigurado => !string.IsNullOrWhiteSpace(StringDeConexao);

    public const string MotivoDoPulo =
        "Sem string de conexão de teste. Defina a variável de ambiente "
        + "ConnectionStrings__TestesIntegracao, ou tenha o appsettings.Development.json "
        + "preenchido em src/Praxis.Api.";

    private static string? Descobrir()
    {
        var doAmbiente = Environment.GetEnvironmentVariable("ConnectionStrings__TestesIntegracao")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Principal");

        if (!string.IsNullOrWhiteSpace(doAmbiente))
        {
            return doAmbiente;
        }

        return LerDoAppSettingsLocal();
    }

    private static string? LerDoAppSettingsLocal()
    {
        var diretorio = new DirectoryInfo(AppContext.BaseDirectory);

        while (diretorio is not null)
        {
            var caminho = Path.Combine(
                diretorio.FullName, "src", "Praxis.Api", "appsettings.Development.json");

            if (File.Exists(caminho))
            {
                return LerConexao(caminho);
            }

            diretorio = diretorio.Parent;
        }

        return null;
    }

    private static string? LerConexao(string caminho)
    {
        try
        {
            using var documento = JsonDocument.Parse(File.ReadAllText(caminho));

            if (!documento.RootElement.TryGetProperty("ConnectionStrings", out var secao))
            {
                return null;
            }

            if (secao.TryGetProperty("TestesIntegracao", out var testes)
                && !string.IsNullOrWhiteSpace(testes.GetString()))
            {
                return testes.GetString();
            }

            return secao.TryGetProperty("Principal", out var principal) ? principal.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
