using Praxis.Modules.Assinaturas.Domain;

namespace Praxis.Modules.Assinaturas.Application;

/// <summary>
/// O que atravessa a fronteira do módulo. Entidade de domínio nunca sai daqui —
/// se ela vazasse, os outros módulos passariam a depender de regra que não é
/// deles e a fronteira morreria em silêncio.
/// </summary>
public sealed record DireitoDeUsoDto(
    Produto Produto,
    DateTimeOffset InicioEm,
    DateTimeOffset? FimEm);
