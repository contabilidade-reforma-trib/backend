namespace Praxis.Modules.Assinaturas.Domain;

/// <summary>
/// Os dois produtos, vendidos separadamente. Nenhum depende do outro para
/// funcionar — ver D-02 em docs/decisoes.md.
/// </summary>
public enum Produto
{
    Copiloto = 1,
    Mentoria = 2,
}
