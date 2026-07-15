using HungryUp.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace HungryUp.Persistence.Payroll;

public class PayrollDbContext : DbContext
{
    public PayrollDbContext(DbContextOptions<PayrollDbContext> options) : base(options) { }

    public DbSet<Empleado> Empleados => Set<Empleado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empleado>(e =>
        {
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Documento).HasMaxLength(40);
            e.Property(x => x.Cargo).HasMaxLength(120).IsRequired();
            e.Property(x => x.SalarioMensual).HasPrecision(18, 2);
        });
    }
}
