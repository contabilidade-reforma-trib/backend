using Praxis.Modules.Assinaturas.Domain;
using Praxis.UnitTests.Infra;
using Xunit;

namespace Praxis.UnitTests.Assinaturas.Domain;

public class AssinaturaTests
{
    private static readonly TimeSpan UmAno = TimeSpan.FromDays(365);

    [Fact]
    public void Deve_conceder_acesso_apenas_ao_produto_comprado()
    {
        var relogio = RelogioFalso.Em(2026, 9, 5);
        var assinatura = Assinatura.Criar(Guid.NewGuid(), relogio);

        assinatura.ConcederAcesso(Produto.Copiloto, UmAno, relogio);

        Assert.True(assinatura.PossuiAcessoVigente(Produto.Copiloto, relogio.AgoraUtc));
        Assert.False(assinatura.PossuiAcessoVigente(Produto.Mentoria, relogio.AgoraUtc));
    }

    [Fact]
    public void Deve_permitir_comprar_o_segundo_produto_depois_sem_mexer_no_primeiro()
    {
        var relogio = RelogioFalso.Em(2026, 9, 5);
        var assinatura = Assinatura.Criar(Guid.NewGuid(), relogio);
        assinatura.ConcederAcesso(Produto.Copiloto, UmAno, relogio);

        relogio.Avancar(TimeSpan.FromDays(90));
        assinatura.ConcederAcesso(Produto.Mentoria, UmAno, relogio);

        Assert.True(assinatura.PossuiAcessoVigente(Produto.Copiloto, relogio.AgoraUtc));
        Assert.True(assinatura.PossuiAcessoVigente(Produto.Mentoria, relogio.AgoraUtc));
        Assert.Equal(2, assinatura.DireitosDeUso.Count);
    }

    [Fact]
    public void Deve_estender_a_vigencia_em_vez_de_criar_direito_duplicado()
    {
        var relogio = RelogioFalso.Em(2026, 9, 5);
        var assinatura = Assinatura.Criar(Guid.NewGuid(), relogio);
        var primeiro = assinatura.ConcederAcesso(Produto.Copiloto, UmAno, relogio);
        var fimOriginal = primeiro.FimEm;

        relogio.Avancar(TimeSpan.FromDays(30));
        assinatura.ConcederAcesso(Produto.Copiloto, UmAno, relogio);

        // Quem assina o Combo já tendo Copiloto ativo não pode perder o que pagou:
        // o período novo soma ao que restava, e continua existindo um único direito.
        Assert.Single(assinatura.DireitosDeUso);
        Assert.Equal(fimOriginal!.Value.Add(UmAno), assinatura.DireitosDeUso.Single().FimEm);
    }

    [Fact]
    public void Deve_deixar_de_ter_acesso_quando_a_vigencia_termina()
    {
        var relogio = RelogioFalso.Em(2026, 9, 5);
        var assinatura = Assinatura.Criar(Guid.NewGuid(), relogio);
        assinatura.ConcederAcesso(Produto.Mentoria, TimeSpan.FromDays(30), relogio);

        relogio.Avancar(TimeSpan.FromDays(31));

        Assert.False(assinatura.PossuiAcessoVigente(Produto.Mentoria, relogio.AgoraUtc));
    }

    [Fact]
    public void Deve_nascer_pendente_e_so_ficar_ativa_quando_ativada()
    {
        var relogio = RelogioFalso.Em(2026, 9, 5);
        var assinatura = Assinatura.Criar(Guid.NewGuid(), relogio);

        Assert.Equal(SituacaoDaAssinatura.Pendente, assinatura.Situacao);

        assinatura.Ativar(relogio);

        Assert.Equal(SituacaoDaAssinatura.Ativa, assinatura.Situacao);
    }

    [Fact]
    public void Deve_registrar_pagamento_como_nao_confirmado_ate_a_confirmacao()
    {
        var relogio = RelogioFalso.Em(2026, 9, 5);
        var assinatura = Assinatura.Criar(Guid.NewGuid(), relogio);

        var pagamento = assinatura.RegistrarPagamento(MeioDePagamento.Pix, 1890m, "Combo anual", relogio);

        Assert.False(pagamento.Confirmado);
        Assert.Null(pagamento.ConfirmadoEm);

        pagamento.Confirmar(relogio);

        Assert.True(pagamento.Confirmado);
        Assert.Equal(relogio.AgoraUtc, pagamento.ConfirmadoEm);
    }
}
