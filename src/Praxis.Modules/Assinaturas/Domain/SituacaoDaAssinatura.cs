namespace Praxis.Modules.Assinaturas.Domain;

public enum SituacaoDaAssinatura
{
    /// <summary>Criada, aguardando confirmação de pagamento.</summary>
    Pendente = 1,

    Ativa = 2,

    /// <summary>Pagamento em atraso. O acesso segue a vigência dos direitos de uso.</summary>
    Inadimplente = 3,

    Cancelada = 4,
}
