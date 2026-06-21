using HungryUp.Domain.Auth;
using Microsoft.EntityFrameworkCore;

namespace HungryUp.Persistence.Auth;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Username).IsRequired().HasMaxLength(50);
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.Property(u => u.FullName).IsRequired().HasMaxLength(150);
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.EnterpriseName).HasMaxLength(150);
            e.Property(u => u.Rol).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasIndex(r => r.TokenHash);
            e.Property(r => r.TokenHash).IsRequired().HasMaxLength(128);
            e.Property(r => r.CreatedByIp).HasMaxLength(64);
            e.Property(r => r.UserAgent).HasMaxLength(512);
            e.HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
