using Praxis.Modules.Identidade.Domain;
using Praxis.UnitTests.Infra;
using Xunit;

namespace Praxis.UnitTests.Identidade.Domain;

public class OrganizacaoTests
{
    private readonly RelogioFalso relogio = RelogioFalso.Em(2026, 9, 5);

    [Fact]
    public void Deve_criar_organizacao_com_cnpj_valido()
    {
        var resultado = Organizacao.Criar("Escritório Contábil Ltda", "12.345.678/0001-90", relogio);

        Assert.True(resultado.EstaOk);
        Assert.Equal("12345678000190", resultado.Valor.Documento);
        Assert.Equal("Escritório Contábil Ltda", resultado.Valor.RazaoSocial);
    }

    [Fact]
    public void Deve_aceitar_cpf_porque_contador_autonomo_tambem_e_uma_organizacao()
    {
        var resultado = Organizacao.Criar("Aline Bertoni", "123.456.789-09", relogio);

        Assert.True(resultado.EstaOk);
        Assert.Equal("12345678909", resultado.Valor.Documento);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234567890123456")]
    [InlineData("")]
    public void Deve_recusar_documento_que_nao_seja_cpf_nem_cnpj(string documento)
    {
        var resultado = Organizacao.Criar("Escritório Contábil Ltda", documento, relogio);

        Assert.True(resultado.Falhou);
        Assert.Equal("organizacao.documento_invalido", resultado.Erro.Codigo);
    }

    [Fact]
    public void Deve_recusar_razao_social_vazia()
    {
        var resultado = Organizacao.Criar("   ", "12345678000190", relogio);

        Assert.True(resultado.Falhou);
        Assert.Equal("organizacao.razao_social_vazia", resultado.Erro.Codigo);
    }

    [Fact]
    public void Deve_adicionar_usuario_normalizando_o_email()
    {
        var organizacao = Organizacao.Criar("Escritório", "12345678000190", relogio).Valor;

        var resultado = organizacao.AdicionarUsuario(
            "Aline Bertoni",
            "  Aline.Bertoni@Escritorio.COM.BR ",
            PapelDoUsuario.Proprietario,
            relogio);

        Assert.True(resultado.EstaOk);
        Assert.Equal("aline.bertoni@escritorio.com.br", resultado.Valor.Email);
        Assert.Single(organizacao.Usuarios);
    }

    [Fact]
    public void Deve_recusar_segundo_usuario_com_o_mesmo_email_na_organizacao()
    {
        var organizacao = Organizacao.Criar("Escritório", "12345678000190", relogio).Valor;
        organizacao.AdicionarUsuario("Aline", "aline@escritorio.com.br", PapelDoUsuario.Proprietario, relogio);

        var resultado = organizacao.AdicionarUsuario(
            "Aline de novo",
            "ALINE@escritorio.com.br",
            PapelDoUsuario.Membro,
            relogio);

        Assert.True(resultado.Falhou);
        Assert.Equal("organizacao.email_duplicado", resultado.Erro.Codigo);
        Assert.Single(organizacao.Usuarios);
    }

    [Theory]
    [InlineData("sem-arroba")]
    [InlineData("sem@ponto")]
    [InlineData("@dominio.com")]
    [InlineData("")]
    public void Deve_recusar_email_invalido(string email)
    {
        var organizacao = Organizacao.Criar("Escritório", "12345678000190", relogio).Valor;

        var resultado = organizacao.AdicionarUsuario("Aline", email, PapelDoUsuario.Membro, relogio);

        Assert.True(resultado.Falhou);
        Assert.Equal("usuario.email_invalido", resultado.Erro.Codigo);
    }
}
