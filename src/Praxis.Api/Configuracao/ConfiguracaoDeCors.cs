namespace Praxis.Api.Configuracao;

/// <summary>
/// O front roda na Vercel, em outro domínio, então sem CORS o healthcheck do
/// front nem sai do navegador. As origens vêm da configuração para que
/// adicionar um domínio de preview não exija recompilar.
/// </summary>
public static class ConfiguracaoDeCors
{
    public const string NomeDaPolitica = "frontend";

    public static IServiceCollection AdicionarCorsDoFrontend(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        var origens = configuracao.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? [];

        servicos.AddCors(opcoes => opcoes.AddPolicy(NomeDaPolitica, politica =>
        {
            if (origens.Length == 0)
            {
                // Sem lista configurada, a POC aceita qualquer origem — vale para
                // subir e testar rápido, e é a primeira coisa a fechar quando o
                // produto deixar de ser protótipo.
                politica.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                return;
            }

            politica.WithOrigins(origens)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }));

        return servicos;
    }
}
