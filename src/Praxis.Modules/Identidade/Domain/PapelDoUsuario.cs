namespace Praxis.Modules.Identidade.Domain;

public enum PapelDoUsuario
{
    /// <summary>Quem contratou. Responde pela assinatura e pode convidar membros.</summary>
    Proprietario = 1,

    /// <summary>Funcionário do escritório, usa a plataforma mas não mexe na assinatura.</summary>
    Membro = 2,
}
