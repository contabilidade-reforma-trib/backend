using Praxis.Shared.Abstracoes;

namespace Praxis.UnitTests.Infra;

/// <summary>
/// Relógio controlado pelo teste. É o que permite verificar vigência de
/// assinatura sem esperar o calendário.
/// </summary>
public sealed class RelogioFalso : IRelogio
{
    public RelogioFalso(DateTimeOffset agora) => AgoraUtc = agora;

    public DateTimeOffset AgoraUtc { get; private set; }

    public static RelogioFalso Em(int ano, int mes, int dia) =>
        new(new DateTimeOffset(ano, mes, dia, 12, 0, 0, TimeSpan.Zero));

    public void Avancar(TimeSpan periodo) => AgoraUtc = AgoraUtc.Add(periodo);
}
