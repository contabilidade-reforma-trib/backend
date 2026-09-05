using Xunit;

namespace Praxis.IntegrationTests.Infra;

/// <summary>
/// Igual a <see cref="FactAttribute"/>, mas se pula sozinho quando não há banco
/// de teste configurado. O xUnit 2 decide o pulo na construção do atributo, por
/// isso a verificação acontece aqui e não dentro do teste.
/// </summary>
public sealed class FatoDeIntegracaoAttribute : FactAttribute
{
    public FatoDeIntegracaoAttribute()
    {
        if (!ConfiguracaoDeTeste.EstaConfigurado)
        {
            Skip = ConfiguracaoDeTeste.MotivoDoPulo;
        }
    }
}
