using HungryUp.Application.Payroll.Dtos;

namespace HungryUp.Application.Payroll;

public interface IPayrollService
{
    Task<List<EmpleadoDto>> GetEmpleadosAsync(bool soloActivos = false);
    Task<EmpleadoDto?> ObtenerPorIdAsync(Guid id);
    Task<EmpleadoDto> CrearAsync(CreateEmpleadoDto dto);
    Task<EmpleadoDto> ActualizarAsync(Guid id, UpdateEmpleadoDto dto);
    Task EliminarAsync(Guid id);

    /// <summary>
    /// Calcula la nómina prorrateada de los empleados activos para el rango [desde, hasta]:
    /// por cada empleado, salarioMensual / 30 × días del rango.
    /// </summary>
    Task<NominaDto> CalcularNominaAsync(DateTime desde, DateTime hasta);
}
