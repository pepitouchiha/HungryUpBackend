using HungryUp.Api.Authorization;
using HungryUp.Application.Auth;
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
    [HasPermission(Permissions.Categories.Read)]
    public async Task<IActionResult> GetCategorias([FromQuery] bool activos = false) =>
        Ok(await _service.GetCategoriasAsync(activos));

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Categories.Read)]
    public async Task<IActionResult> GetCategoria(Guid id)
    {
        var categoria = await _service.ObtenerCategoriaPorIdAsync(id);
        return categoria is null ? NotFound() : Ok(categoria);
    }

    [HttpPost]
    [HasPermission(Permissions.Categories.Create)]
    public async Task<IActionResult> CreateCategoria([FromBody] CreateCategoriaDto dto)
    {
        var categoria = await _service.CrearCategoriaAsync(dto);
        return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Categories.Update)]
    public async Task<IActionResult> UpdateCategoria(Guid id, [FromBody] UpdateCategoriaDto dto) =>
        Ok(await _service.ActualizarCategoriaAsync(id, dto));

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Categories.Delete)]
    public async Task<IActionResult> DeleteCategoria(Guid id)
    {
        await _service.EliminarCategoriaAsync(id);
        return NoContent();
    }
}
