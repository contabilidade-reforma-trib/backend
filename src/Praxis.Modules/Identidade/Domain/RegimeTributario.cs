namespace Praxis.Modules.Identidade.Domain;

/// <summary>
/// Regime da maior parte da carteira. Segunda pergunta do cadastro, e contexto
/// que o Copiloto usa para não responder com a regra do regime errado.
/// </summary>
public enum RegimeTributario
{
    SimplesNacional = 1,
    LucroPresumido = 2,
    LucroReal = 3,
}
