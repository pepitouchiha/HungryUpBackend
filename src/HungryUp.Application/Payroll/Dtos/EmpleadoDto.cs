namespace HungryUp.Application.Payroll.Dtos;

public record EmpleadoDto(
    Guid Id,
    string Nombre,
    string? Documento,
    string Cargo,
    decimal SalarioMensual,
    DateTime FechaIngreso,
    bool Activo);

public record CreateEmpleadoDto(
    string Nombre,
    string? Documento,
    string Cargo,
    decimal SalarioMensual,
    DateTime? FechaIngreso);

public record UpdateEmpleadoDto(
    string Nombre,
    string? Documento,
    string Cargo,
    decimal SalarioMensual,
    DateTime FechaIngreso,
    bool Activo);

/// <summary>Resultado del cálculo de nómina prorrateada para un rango de fechas.</summary>
public record NominaDto(decimal Total, int Empleados, int Dias);
