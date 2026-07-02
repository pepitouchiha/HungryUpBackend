using HungryUp.Application.Orders.Dtos;

namespace HungryUp.Application.Orders;

public interface IMesaService
{
    Task<List<MesaDto>> GetMesasAsync(bool soloActivos = false);
    Task<MesaDto?> ObtenerPorIdAsync(Guid id);
    Task<MesaDto> CrearAsync(CreateMesaDto dto);
    Task<MesaDto> ActualizarAsync(Guid id, UpdateMesaDto dto);
    Task EliminarAsync(Guid id);
}
