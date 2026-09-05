namespace Praxis.Api.Configuracao;

/// <summary>
/// Nenhum navegador chama esta API diretamente: o front usa o servidor do Next
/// como BFF, e é ele quem fala com o backend. Chamada de servidor para servidor
/// não passa por CORS — ver D-14 em docs/decisoes.md.
///
/// A política existe como escape para casos pontuais (uma ferramenta externa,
/// um teste manual), e por isso **lista vazia significa nenhuma origem
/// permitida**. Liberar geral no silêncio da falta de configuração seria abrir
/// o backend para qualquer site sem ninguém decidir isso.
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
