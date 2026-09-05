using Microsoft.EntityFrameworkCore;
using Praxis.Identity.Domain;

namespace Praxis.Identity.Infrastructure;

public static class IdentityMapping
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("identity_users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Id).ValueGeneratedNever();
            entity.Property(user => user.Name).HasMaxLength(200).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Phone).HasMaxLength(40);
            entity.Property(user => user.CreatedAt).IsRequired();
            entity.Property(user => user.UpdatedAt).IsRequired();
        });
    }
}
