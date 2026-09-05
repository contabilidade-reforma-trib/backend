using Microsoft.EntityFrameworkCore;
using Praxis.IntegrationTests.Infra;
using Praxis.Modules.Assinaturas.Domain;
using Praxis.Modules.Assinaturas.Infrastructure;
using Praxis.Modules.Identidade.Domain;
using Praxis.Shared.Abstracoes;
using Xunit;

namespace Praxis.IntegrationTests.Assinaturas;

/// <summary>
/// Exercita o contrato público do módulo Assinaturas contra o banco de verdade.
/// É aqui que se verifica que o mapeamento, o filtro de vigência e a projeção
/// para DTO funcionam juntos — coisa que teste de unidade não alcança.
/// </summary>
public class ConsultaDeDireitoDeUsoTests : IClassFixture<BancoDeTesteFixture>
{
    private static readonly TimeSpan UmAno = TimeSpan.FromDays(365);

    private readonly BancoDeTesteFixture banco;
    private readonly IRelogio relogio = new RelogioDoSistema();

    public ConsultaDeDireitoDeUsoTests(BancoDeTesteFixture banco) => this.banco = banco;

    [FatoDeIntegracao]
    public async Task Deve_criar_o_schema_isolado_com_as_tabelas_dos_dois_modulos()
    {
        await using var contexto = banco.CriarContexto();

        Assert.True(await contexto.Database.CanConnectAsync());
        Assert.StartsWith("teste_", banco.Schema);

        // As tabelas têm de existir DENTRO do schema da execução, não no public —
        // é o que garante que uma suíte não suja o banco principal nem a outra.
        var tabelas = await ListarTabelasDoSchema();

        Assert.Contains("identidade_organizacao", tabelas);
        Assert.Contains("identidade_usuario", tabelas);
        Assert.Contains("identidade_perfil_de_uso", tabelas);
        Assert.Contains("assinaturas_assinatura", tabelas);
        Assert.Contains("assinaturas_direito_de_uso", tabelas);
        Assert.Contains("assinaturas_pagamento", tabelas);
    }

    private async Task<List<string>> ListarTabelasDoSchema()
    {
        await using var conexao = new Npgsql.NpgsqlConnection(banco.StringDeConexao);
        await conexao.OpenAsync();

        await using var comando = new Npgsql.NpgsqlCommand(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = @schema",
            conexao);
        comando.Parameters.AddWithValue("schema", banco.Schema);

        var tabelas = new List<string>();
        await using var leitor = await comando.ExecuteReaderAsync();

        while (await leitor.ReadAsync())
        {
            tabelas.Add(leitor.GetString(0));
        }

        return tabelas;
    }

    [FatoDeIntegracao]
    public async Task Deve_responder_que_possui_acesso_ao_produto_comprado()
    {
        var organizacaoId = await SemearOrganizacaoComAcesso(Produto.Copiloto);

        await using var contexto = banco.CriarContexto();
        var consulta = new ConsultaDeDireitoDeUso(contexto, relogio);

        Assert.True(await consulta.PossuiAcesso(organizacaoId, Produto.Copiloto, default));
        Assert.False(await consulta.PossuiAcesso(organizacaoId, Produto.Mentoria, default));
    }

    [FatoDeIntegracao]
    public async Task Deve_devolver_dto_e_nunca_entidade_de_dominio()
    {
        var organizacaoId = await SemearOrganizacaoComAcesso(Produto.Mentoria);

        await using var contexto = banco.CriarContexto();
        var consulta = new ConsultaDeDireitoDeUso(contexto, relogio);

        var vigentes = await consulta.ListarVigentes(organizacaoId, default);

        var direito = Assert.Single(vigentes);
        Assert.Equal(Produto.Mentoria, direito.Produto);
        Assert.NotNull(direito.FimEm);
    }

    [FatoDeIntegracao]
    public async Task Nao_deve_enxergar_acesso_de_assinatura_cancelada()
    {
        var organizacaoId = await SemearOrganizacaoComAcesso(Produto.Copiloto, cancelar: true);

        await using var contexto = banco.CriarContexto();
        var consulta = new ConsultaDeDireitoDeUso(contexto, relogio);

        Assert.False(await consulta.PossuiAcesso(organizacaoId, Produto.Copiloto, default));
    }

    [FatoDeIntegracao]
    public async Task Nao_deve_enxergar_direito_com_vigencia_expirada()
    {
        var organizacaoId = Guid.NewGuid();

        await using (var escrita = banco.CriarContexto())
        {
            var assinatura = Assinatura.Criar(organizacaoId, relogio);
            assinatura.Ativar(relogio);
            // Um dia de vigência, concedido "ontem": já venceu quando a consulta roda.
            var relogioDeOntem = new RelogioFixo(relogio.AgoraUtc.AddDays(-2));
            assinatura.ConcederAcesso(Produto.Mentoria, TimeSpan.FromDays(1), relogioDeOntem);

            escrita.Assinaturas.Add(assinatura);
            await escrita.SaveChangesAsync();
        }

        await using var contexto = banco.CriarContexto();
        var consulta = new ConsultaDeDireitoDeUso(contexto, relogio);

        Assert.False(await consulta.PossuiAcesso(organizacaoId, Produto.Mentoria, default));
    }

    [FatoDeIntegracao]
    public async Task Deve_persistir_organizacao_com_usuario_e_perfil()
    {
        var organizacao = Organizacao.Criar("Escritório Praxis", "12345678000190", relogio).Valor;
        var usuario = organizacao.AdicionarUsuario(
            "Aline Bertoni",
            $"aline.{Guid.NewGuid():N}@escritorio.com.br",
            PapelDoUsuario.Proprietario,
            relogio).Valor;

        var perfil = PerfilDeUso.Criar(
            usuario.Id,
            AreaDeAtuacao.EscritorioContabil,
            RegimeTributario.LucroPresumido,
            ["Comércio atacadista", "Indústria de alimentos"],
            "Caixa do cliente na virada",
            relogio).Valor;

        usuario.DefinirPerfil(perfil, relogio);

        await using (var escrita = banco.CriarContexto())
        {
            escrita.Organizacoes.Add(organizacao);
            await escrita.SaveChangesAsync();
        }

        await using var leitura = banco.CriarContexto();
        var lida = await leitura.Organizacoes
            .Include(o => o.Usuarios)
            .ThenInclude(u => u.Perfil)
            .FirstAsync(o => o.Id == organizacao.Id);

        var usuarioLido = Assert.Single(lida.Usuarios);
        Assert.Equal("Aline Bertoni", usuarioLido.Nome);
        Assert.NotNull(usuarioLido.Perfil);
        Assert.Equal(2, usuarioLido.Perfil!.Setores.Count);
        Assert.Contains("Comércio atacadista", usuarioLido.Perfil.Setores);
    }

    private async Task<Guid> SemearOrganizacaoComAcesso(Produto produto, bool cancelar = false)
    {
        var organizacaoId = Guid.NewGuid();

        await using var contexto = banco.CriarContexto();
        var assinatura = Assinatura.Criar(organizacaoId, relogio);
        assinatura.Ativar(relogio);
        assinatura.ConcederAcesso(produto, UmAno, relogio);

        if (cancelar)
        {
            assinatura.Cancelar(relogio);
        }

        contexto.Assinaturas.Add(assinatura);
        await contexto.SaveChangesAsync();

        return organizacaoId;
    }

    private sealed class RelogioFixo(DateTimeOffset agora) : IRelogio
    {
        public DateTimeOffset AgoraUtc { get; } = agora;
    }
}
