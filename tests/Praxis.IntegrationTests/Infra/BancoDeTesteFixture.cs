using Microsoft.EntityFrameworkCore;
using Npgsql;
using Praxis.Modules.Persistencia;
using Xunit;

namespace Praxis.IntegrationTests.Infra;

/// <summary>
/// Cria um schema isolado por execução no mesmo banco Neon, aplica o modelo nele
/// e o derruba ao final — inclusive quando um teste falha ou estoura no meio,
/// porque a derrubada vive em <c>finally</c>.
///
/// O nome carrega o instante de criação (<c>teste_aaaaMMddHHmmss_xxxxxxxx</c>)
/// justamente para permitir a varredura de órfãos: se a máquina cair antes do
/// dispose, a próxima execução limpa o que ficou para trás.
/// </summary>
public sealed class BancoDeTesteFixture : IAsyncLifetime
{
    private const string PrefixoDoSchema = "teste_";
    private static readonly TimeSpan IdadeMaximaDeOrfao = TimeSpan.FromHours(24);

    public string Schema { get; } =
        $"{PrefixoDoSchema}{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}"[..40];

    public string StringDeConexao { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        if (!ConfiguracaoDeTeste.EstaConfigurado)
        {
            return;
        }

        StringDeConexao = ConfiguracaoDeTeste.StringDeConexao!;

        await DerrubarSchemasOrfaos();

        await ExecutarSql($"CREATE SCHEMA IF NOT EXISTS \"{Schema}\";");

        // EnsureCreated não serve aqui: ele decide pela existência do BANCO, e o
        // banco já existe. O schema novo continuaria vazio e todo insert falharia
        // com "relation does not exist". Gerar o script do modelo cria as tabelas
        // dentro do schema isolado, que é o que queremos.
        await using var contexto = CriarContexto();
        await ExecutarSql(contexto.Database.GenerateCreateScript());
    }

    public async Task DisposeAsync()
    {
        if (!ConfiguracaoDeTeste.EstaConfigurado)
        {
            return;
        }

        try
        {
            await ExecutarSql($"DROP SCHEMA IF EXISTS \"{Schema}\" CASCADE;");
        }
        catch (NpgsqlException)
        {
            // Falhar aqui não pode derrubar a suíte inteira. O schema vira órfão
            // e a varredura da próxima execução dá conta dele.
        }
    }

    public PraxisDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<PraxisDbContext>()
            .UseNpgsql(StringDeConexao)
            .Options;

        return new PraxisDbContext(opcoes, Schema);
    }

    private async Task DerrubarSchemasOrfaos()
    {
        var limite = DateTime.UtcNow - IdadeMaximaDeOrfao;

        await using var conexao = new NpgsqlConnection(StringDeConexao);
        await conexao.OpenAsync();

        var nomes = new List<string>();

        await using (var comando = new NpgsqlCommand(
            "SELECT schema_name FROM information_schema.schemata WHERE schema_name LIKE @prefixo",
            conexao))
        {
            comando.Parameters.AddWithValue("prefixo", $"{PrefixoDoSchema}%");
            await using var leitor = await comando.ExecuteReaderAsync();

            while (await leitor.ReadAsync())
            {
                nomes.Add(leitor.GetString(0));
            }
        }

        foreach (var nome in nomes.Where(nome => EhOrfao(nome, limite)))
        {
            await using var comando = new NpgsqlCommand($"DROP SCHEMA IF EXISTS \"{nome}\" CASCADE;", conexao);
            await comando.ExecuteNonQueryAsync();
        }
    }

    private static bool EhOrfao(string nomeDoSchema, DateTime limite)
    {
        var partes = nomeDoSchema.Split('_');

        if (partes.Length < 3
            || !DateTime.TryParseExact(
                partes[1],
                "yyyyMMddHHmmss",
                null,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var criadoEm))
        {
            return false;
        }

        return criadoEm < limite;
    }

    private async Task ExecutarSql(string sql)
    {
        await using var conexao = new NpgsqlConnection(StringDeConexao);
        await conexao.OpenAsync();
        await using var comando = new NpgsqlCommand(sql, conexao);
        await comando.ExecuteNonQueryAsync();
    }
}
