using HungryUpBackend.Modules.Catalog.Services;
using Microsoft.AspNetCore.Mvc;

namespace HungryUpBackend.Modules.Catalog.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly ICatalogService _service;

    public ProductsController(ICatalogService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetProductos() =>
        Ok(await _service.GetProductosActivosAsync());

    [HttpPost]
    public async Task<IActionResult> CreateProducto([FromBody] CreateProductoRequest request)
    {
        var producto = await _service.CrearProductoAsync(
            request.Nombre, request.Precio, request.StockInicial, request.CategoriaId);
        return CreatedAtAction(nameof(GetProductos), new { id = producto.Id }, producto);
    }
}

public record CreateProductoRequest(string Nombre, decimal Precio, int StockInicial, Guid CategoriaId);
