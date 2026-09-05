namespace Praxis.Modules.Assinaturas.Domain;

/// <summary>
/// Pix e boleto não são opcionais para este público — cartão sozinho não atende.
/// Na POC o pagamento é simulado; a escolha do gateway é a decisão D-12, aberta.
/// </summary>
public enum MeioDePagamento
{
    Pix = 1,
    Boleto = 2,
    Cartao = 3,

    /// <summary>Usado enquanto a POC não tem gateway real.</summary>
    Simulado = 99,
}
