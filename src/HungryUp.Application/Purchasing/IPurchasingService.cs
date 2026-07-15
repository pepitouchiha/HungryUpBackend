using HungryUp.Application.Purchasing.Dtos;
using HungryUp.Domain.Purchasing;

namespace HungryUp.Application.Purchasing;

public interface IPurchasingService
{
    Task<List<CompraDto>> GetComprasAsync(EstadoCompra? estado = null);
    Task<CompraDto?> ObtenerPorIdAsync(Guid id);

    /// <summary>Crea una factura de compra en estado Borrador (no afecta el inventario todavía).</summary>
    Task<CompraDto> CrearAsync(CreateCompraDto dto);

    /// <summary>Edita una factura de compra. Solo permitido mientras esté en Borrador.</summary>
    Task<CompraDto> ActualizarAsync(Guid id, UpdateCompraDto dto);

    /// <summary>Actualiza únicamente las notas de la compra.</summary>
    Task<CompraDto> ActualizarNotasAsync(Guid id, UpdateNotasCompraDto dto);

    /// <summary>Confirma la compra: aumenta el inventario de cada producto y bloquea la edición.</summary>
    Task<CompraDto> ConfirmarAsync(Guid id);

    /// <summary>Anula una compra confirmada revirtiendo el inventario que había sumado.</summary>
    Task<CompraDto> AnularAsync(Guid id);

    /// <summary>Elimina una compra en Borrador.</summary>
    Task EliminarAsync(Guid id);

    /// <summary>Total pagado en compras confirmadas en el rango [desde, hasta] (por fecha de confirmación). Para analítica.</summary>
    Task<decimal> GetTotalCompradoAsync(DateTime desde, DateTime hasta);
}
