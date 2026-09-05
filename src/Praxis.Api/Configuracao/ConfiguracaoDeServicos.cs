using Amazon.Runtime;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Praxis.Modules.Assinaturas.Application;
using Praxis.Modules.Assinaturas.Infrastructure;
using Praxis.Modules.Identidade.Application;
using Praxis.Modules.Identidade.Infrastructure;
using Praxis.Modules.Persistencia;
using Praxis.Shared.Abstracoes;
using Praxis.Shared.Persistencia;

namespace Praxis.Api.Configuracao;

public static class ConfiguracaoDeServicos
{
    public static IServiceCollection AdicionarPersistencia(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        var stringDeConexao = configuracao.GetConnectionString("Principal");

        if (string.IsNullOrWhiteSpace(stringDeConexao))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Principal não está configurada. Em desenvolvimento ela vem de "
                + "appsettings.Development.json; no Railway, da variável de ambiente "
                + "ConnectionStrings__Principal.");
        }

        servicos.AddDbContext<PraxisDbContext>(opcoes => opcoes.UseNpgsql(stringDeConexao));
        servicos.AddSingleton<IRelogio, RelogioDoSistema>();

        return servicos;
    }

    public static IServiceCollection AdicionarArmazenamentoDeObjetos(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        servicos.Configure<OpcoesDeArmazenamento>(configuracao.GetSection(OpcoesDeArmazenamento.Secao));

        var opcoes = configuracao.GetSection(OpcoesDeArmazenamento.Secao).Get<OpcoesDeArmazenamento>()
            ?? new OpcoesDeArmazenamento();

        // Sem credencial, a API sobe do mesmo jeito e o healthcheck reporta o
        // armazenamento como não configurado. É melhor que quebrar no boot
        // enquanto as credenciais de vídeo ainda não existem.
        if (!opcoes.EstaConfigurado)
        {
            return servicos;
        }

        servicos.AddSingleton<IAmazonS3>(_ =>
        {
            var configuracaoDoCliente = new AmazonS3Config
            {
                ServiceURL = opcoes.Endpoint,
                // O R2 exige caminho no estilo path, não subdomínio por bucket.
                ForcePathStyle = true,
                AuthenticationRegion = opcoes.Regiao,
            };

            var credenciais = new BasicAWSCredentials(opcoes.AccessKeyId, opcoes.SecretAccessKey);
            return new AmazonS3Client(credenciais, configuracaoDoCliente);
        });

        servicos.AddSingleton<IArmazenamentoDeObjetos, ArmazenamentoEmR2>();

        return servicos;
    }

    public static IServiceCollection AdicionarModulos(this IServiceCollection servicos)
    {
        servicos.AddScoped<IRepositorioDeOrganizacoes, RepositorioDeOrganizacoes>();
        servicos.AddScoped<IRepositorioDeAssinaturas, RepositorioDeAssinaturas>();
        servicos.AddScoped<IConsultaDeDireitoDeUso, ConsultaDeDireitoDeUso>();

        return servicos;
    }

    public static IServiceCollection AdicionarDocumentacao(this IServiceCollection servicos)
    {
        servicos.AddEndpointsApiExplorer();
        servicos.AddSwaggerGen(opcoes =>
        {
            opcoes.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Praxis API",
                Version = "v1",
                Description =
                    "Copiloto fiscal e mentoria em reforma tributária. "
                    + "Protótipo — dados e credenciais são descartáveis.",
            });
        });

        return servicos;
    }
}
