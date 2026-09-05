using Microsoft.EntityFrameworkCore;
using Praxis.Modules.Identidade.Domain;

namespace Praxis.Modules.Identidade.Infrastructure;

/// <summary>
/// Mapeamento das tabelas do módulo Identidade. Tabela prefixada pelo módulo,
/// para que dono e fronteira sejam legíveis direto no banco.
/// </summary>
public static class MapeamentoDeIdentidade
{
    public static void Aplicar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organizacao>(entidade =>
        {
            entidade.ToTable("identidade_organizacao");
            entidade.HasKey(o => o.Id);
            entidade.Property(o => o.RazaoSocial).HasMaxLength(200).IsRequired();
            entidade.Property(o => o.Documento).HasMaxLength(14).IsRequired();
            entidade.HasIndex(o => o.Documento).IsUnique();
            entidade.Property(o => o.CriadoEm).IsRequired();
            entidade.Property(o => o.AtualizadoEm).IsRequired();

            entidade.HasMany(o => o.Usuarios)
                .WithOne()
                .HasForeignKey(u => u.OrganizacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            entidade.Navigation(o => o.Usuarios).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Usuario>(entidade =>
        {
            entidade.ToTable("identidade_usuario");
            entidade.HasKey(u => u.Id);
            entidade.Property(u => u.Nome).HasMaxLength(200).IsRequired();
            entidade.Property(u => u.Email).HasMaxLength(320).IsRequired();
            entidade.HasIndex(u => u.Email).IsUnique();
            entidade.Property(u => u.Papel).HasConversion<int>().IsRequired();
            entidade.Property(u => u.RegistroProfissional).HasMaxLength(60);
            entidade.Property(u => u.Telefone).HasMaxLength(40);

            entidade.HasOne(u => u.Perfil)
                .WithOne()
                .HasForeignKey<PerfilDeUso>(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PerfilDeUso>(entidade =>
        {
            entidade.ToTable("identidade_perfil_de_uso");
            entidade.HasKey(p => p.Id);
            entidade.Property(p => p.AreaDeAtuacao).HasConversion<int>().IsRequired();
            entidade.Property(p => p.RegimePredominante).HasConversion<int>().IsRequired();
            entidade.Property(p => p.DorAtual).HasMaxLength(200).IsRequired();

            // Lista de texto vira array nativo do Postgres — não precisa de tabela
            // filha para uma lista curta que só é lida junto com o perfil.
            entidade.Property(p => p.Setores)
                .HasColumnType("text[]")
                .IsRequired();
        });
    }
}
