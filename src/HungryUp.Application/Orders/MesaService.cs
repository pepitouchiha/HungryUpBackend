using HungryUp.Application.Orders.Dtos;
using HungryUp.Domain.Orders;
using HungryUp.Persistence.Orders;
using Microsoft.EntityFrameworkCore;

namespace HungryUp.Application.Orders;

public class MesaService : IMesaService
{
    private readonly OrdersDbContext _db;

    public MesaService(OrdersDbContext db) => _db = db;

    public Task<List<MesaDto>> GetMesasAsync(bool soloActivos = false)
    {
        var query = _db.Mesas.AsQueryable();
        if (soloActivos)
            query = query.Where(m => m.Activo);
        return query
            .OrderBy(m => m.Numero)
            .Select(m => new MesaDto(m.Id, m.Numero, m.Estado, m.Activo))
            .ToListAsync();
    }

    public async Task<MesaDto?> ObtenerPorIdAsync(Guid id)
    {
        var m = await _db.Mesas.FindAsync(id);
        return m is null ? null : Map(m);
    }

    public async Task<MesaDto> CrearAsync(CreateMesaDto dto)
    {
        if (dto.Numero <= 0)
            throw new ArgumentException("El número de mesa debe ser mayor que cero.");

        var duplicada = await _db.Mesas.AnyAsync(m => m.Numero == dto.Numero && m.Activo);
        if (duplicada)
            throw new InvalidOperationException($"Ya existe una mesa activa con el número {dto.Numero}.");

        var mesa = new Mesa
        {
            Id = Guid.NewGuid(),
            Numero = dto.Numero,
            Estado = EstadoMesa.Libre,
            Activo = true
        };
        _db.Mesas.Add(mesa);
        await _db.SaveChangesAsync();
        return Map(mesa);
    }

    public async Task<MesaDto> ActualizarAsync(Guid id, UpdateMesaDto dto)
    {
        var mesa = await _db.Mesas.FindAsync(id)
            ?? throw new KeyNotFoundException($"No existe la mesa {id}.");

        var duplicada = await _db.Mesas.AnyAsync(m => m.Numero == dto.Numero && m.Activo && m.Id != id);
        if (duplicada)
            throw new InvalidOperationException($"Ya existe otra mesa activa con el número {dto.Numero}.");

        mesa.Numero = dto.Numero;
        mesa.Estado = dto.Estado;
        mesa.Activo = dto.Activo;
        await _db.SaveChangesAsync();
        return Map(mesa);
    }

    public async Task EliminarAsync(Guid id)
    {
        var mesa = await _db.Mesas.FindAsync(id)
            ?? throw new KeyNotFoundException($"No existe la mesa {id}.");

        mesa.Activo = false; // borrado lógico
        await _db.SaveChangesAsync();
    }

    private static MesaDto Map(Mesa m) => new(m.Id, m.Numero, m.Estado, m.Activo);
}
