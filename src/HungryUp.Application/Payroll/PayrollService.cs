using HungryUp.Application.Payroll.Dtos;
using HungryUp.Domain.Payroll;
using HungryUp.Persistence.Payroll;
using Microsoft.EntityFrameworkCore;

namespace HungryUp.Application.Payroll;

public class PayrollService : IPayrollService
{
    private readonly PayrollDbContext _db;

    public PayrollService(PayrollDbContext db) => _db = db;

    public Task<List<EmpleadoDto>> GetEmpleadosAsync(bool soloActivos = false)
    {
        var query = _db.Empleados.AsQueryable();
        if (soloActivos)
            query = query.Where(e => e.Activo);
        return query
            .OrderBy(e => e.Nombre)
            .Select(e => new EmpleadoDto(e.Id, e.Nombre, e.Documento, e.Cargo, e.SalarioMensual, e.FechaIngreso, e.Activo))
            .ToListAsync();
    }

    public async Task<EmpleadoDto?> ObtenerPorIdAsync(Guid id)
    {
        var e = await _db.Empleados.FindAsync(id);
        return e is null ? null : Map(e);
    }

    public async Task<EmpleadoDto> CrearAsync(CreateEmpleadoDto dto)
    {
        Validar(dto.Nombre, dto.Cargo, dto.SalarioMensual);

        var empleado = new Empleado
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Documento = string.IsNullOrWhiteSpace(dto.Documento) ? null : dto.Documento.Trim(),
            Cargo = dto.Cargo.Trim(),
            SalarioMensual = dto.SalarioMensual,
            FechaIngreso = dto.FechaIngreso ?? DateTime.UtcNow,
            Activo = true
        };

        _db.Empleados.Add(empleado);
        await _db.SaveChangesAsync();
        return Map(empleado);
    }

    public async Task<EmpleadoDto> ActualizarAsync(Guid id, UpdateEmpleadoDto dto)
    {
        var empleado = await _db.Empleados.FindAsync(id)
            ?? throw new KeyNotFoundException($"No existe el empleado {id}.");

        Validar(dto.Nombre, dto.Cargo, dto.SalarioMensual);

        empleado.Nombre = dto.Nombre.Trim();
        empleado.Documento = string.IsNullOrWhiteSpace(dto.Documento) ? null : dto.Documento.Trim();
        empleado.Cargo = dto.Cargo.Trim();
        empleado.SalarioMensual = dto.SalarioMensual;
        empleado.FechaIngreso = dto.FechaIngreso;
        empleado.Activo = dto.Activo;

        await _db.SaveChangesAsync();
        return Map(empleado);
    }

    public async Task EliminarAsync(Guid id)
    {
        var empleado = await _db.Empleados.FindAsync(id)
            ?? throw new KeyNotFoundException($"No existe el empleado {id}.");

        empleado.Activo = false; // borrado lógico
        await _db.SaveChangesAsync();
    }

    public async Task<NominaDto> CalcularNominaAsync(DateTime desde, DateTime hasta)
    {
        if (hasta < desde)
            throw new ArgumentException("La fecha 'hasta' no puede ser anterior a 'desde'.");

        // Días del rango, inclusivo (ej. 1–31 de julio = 31 días).
        var dias = (hasta.Date - desde.Date).Days + 1;

        var salarios = await _db.Empleados
            .Where(e => e.Activo)
            .Select(e => e.SalarioMensual)
            .ToListAsync();

        // Prorrateo: salarioMensual / 30 × días.
        var total = salarios.Sum(s => Math.Round(s / 30m * dias, 2, MidpointRounding.AwayFromZero));
        return new NominaDto(total, salarios.Count, dias);
    }

    private static void Validar(string nombre, string cargo, decimal salario)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del empleado es obligatorio.");
        if (string.IsNullOrWhiteSpace(cargo))
            throw new ArgumentException("El cargo del empleado es obligatorio.");
        if (salario < 0)
            throw new ArgumentException("El salario no puede ser negativo.");
    }

    private static EmpleadoDto Map(Empleado e) =>
        new(e.Id, e.Nombre, e.Documento, e.Cargo, e.SalarioMensual, e.FechaIngreso, e.Activo);
}
