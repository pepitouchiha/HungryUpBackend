using HungryUp.Application.Catalog;
using HungryUp.Application.Catalog.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace HungryUp.Api.Catalog;

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
    public async Task<IActionResult> CreateProducto([FromBody] CreateProductoDto dto)
    {
        var producto = await _service.CrearProductoAsync(dto);
        return CreatedAtAction(nameof(GetProductos), new { id = producto.Id }, producto);
    }
}
