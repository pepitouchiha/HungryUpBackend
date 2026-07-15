namespace HungryUp.Domain.Payroll;

/// <summary>Empleado con salario mensual. Base para la deducción de nómina en el reporte de ganancias/pérdidas.</summary>
public class Empleado
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Documento/identificación del empleado. Opcional.</summary>
    public string? Documento { get; set; }

    public string Cargo { get; set; } = string.Empty;

    /// <summary>Salario mensual en COP.</summary>
    public decimal SalarioMensual { get; set; }

    public DateTime FechaIngreso { get; set; }

    /// <summary>Borrado lógico: false = inactivo, no entra en el cálculo de nómina.</summary>
    public bool Activo { get; set; } = true;
}
