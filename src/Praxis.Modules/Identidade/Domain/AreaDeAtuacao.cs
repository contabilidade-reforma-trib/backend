namespace Praxis.Modules.Identidade.Domain;

/// <summary>Primeira pergunta do cadastro: onde a pessoa atua.</summary>
public enum AreaDeAtuacao
{
    /// <summary>Atende várias empresas como cliente.</summary>
    EscritorioContabil = 1,

    /// <summary>Faz parte do time fiscal de uma empresa.</summary>
    ContabilidadeInterna = 2,

    /// <summary>Trabalha por projeto ou parecer.</summary>
    Consultoria = 3,
}
