using Microsoft.EntityFrameworkCore;
using Praxis.Modules.Assinaturas.Domain;

namespace Praxis.Modules.Assinaturas.Infrastructure;

/// <summary>
/// Mapeamento das tabelas do módulo Assinaturas.
/// Não há chave estrangeira para <c>identidade_organizacao</c> de propósito:
/// a referência é por identificador, não por relacionamento de banco, para que
/// a fronteira entre módulos continue valendo também no schema.
/// </summary>
public static class MapeamentoDeAssinaturas
{
    public static void Aplicar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assinatura>(entidade =>
        {
            entidade.ToTable("assinaturas_assinatura");
            entidade.HasKey(a => a.Id);
            entidade.Property(a => a.OrganizacaoId).IsRequired();
            entidade.HasIndex(a => a.OrganizacaoId);
            entidade.Property(a => a.Situacao).HasConversion<int>().IsRequired();

            entidade.HasMany(a => a.DireitosDeUso)
                .WithOne()
                .HasForeignKey(d => d.AssinaturaId)
                .OnDelete(DeleteBehavior.Cascade);

            entidade.HasMany(a => a.Pagamentos)
                .WithOne()
                .HasForeignKey(p => p.AssinaturaId)
                .OnDelete(DeleteBehavior.Cascade);

            entidade.Navigation(a => a.DireitosDeUso).UsePropertyAccessMode(PropertyAccessMode.Field);
            entidade.Navigation(a => a.Pagamentos).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<DireitoDeUso>(entidade =>
        {
            entidade.ToTable("assinaturas_direito_de_uso");
            entidade.HasKey(d => d.Id);
            entidade.Property(d => d.Produto).HasConversion<int>().IsRequired();
            entidade.Property(d => d.InicioEm).IsRequired();
            entidade.Property(d => d.FimEm);
            entidade.HasIndex(d => new { d.AssinaturaId, d.Produto });
        });

        modelBuilder.Entity<Pagamento>(entidade =>
        {
            entidade.ToTable("assinaturas_pagamento");
            entidade.HasKey(p => p.Id);
            entidade.Property(p => p.Meio).HasConversion<int>().IsRequired();
            entidade.Property(p => p.Valor).HasPrecision(12, 2).IsRequired();
            entidade.Property(p => p.Descricao).HasMaxLength(200).IsRequired();
            entidade.Property(p => p.Confirmado).IsRequired();
        });
    }
}
