namespace Praxis.Shared.Abstracoes;

/// <summary>
/// Tempo entra por aqui, nunca por <c>DateTimeOffset.UtcNow</c> direto.
/// É o que permite testar vigência de assinatura sem esperar o calendário.
/// </summary>
public interface IRelogio
{
    DateTimeOffset AgoraUtc { get; }
}

public sealed class RelogioDoSistema : IRelogio
{
    public DateTimeOffset AgoraUtc => DateTimeOffset.UtcNow;
}
