using HungryUp.Application.Catalog;
using HungryUp.Application.Catalog.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HungryUp.Api.Catalog;

[ApiController]
[Route("api/v1/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICatalogService _service;

    public CategoriesController(ICatalogService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetCategorias([FromQuery] bool activos = false) =>
        Ok(await _service.GetCategoriasAsync(activos));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategoria(Guid id)
    {
        var categoria = await _service.ObtenerCategoriaPorIdAsync(id);
        return categoria is null ? NotFound() : Ok(categoria);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategoria([FromBody] CreateCategoriaDto dto)
    {
        var categoria = await _service.CrearCategoriaAsync(dto);
        return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategoria(Guid id, [FromBody] UpdateCategoriaDto dto) =>
        Ok(await _service.ActualizarCategoriaAsync(id, dto));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategoria(Guid id)
    {
        await _service.EliminarCategoriaAsync(id);
        return NoContent();
    }
}
