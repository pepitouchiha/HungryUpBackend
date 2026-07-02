using HungryUp.Application.Catalog;
using HungryUp.Application.Catalog.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HungryUp.Api.Catalog;

[ApiController]
[Route("api/v1/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

    private readonly ICatalogService _service;
    private readonly IWebHostEnvironment _env;

    public ProductsController(ICatalogService service, IWebHostEnvironment env)
    {
        _service = service;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetProductos([FromQuery] bool activos = false) =>
        Ok(await _service.GetProductosAsync(activos));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProducto(Guid id)
    {
        var producto = await _service.ObtenerProductoPorIdAsync(id);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProducto([FromBody] CreateProductoDto dto)
    {
        var producto = await _service.CrearProductoAsync(dto);
        return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProducto(Guid id, [FromBody] UpdateProductoDto dto) =>
        Ok(await _service.ActualizarProductoAsync(id, dto));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProducto(Guid id)
    {
        await _service.EliminarProductoAsync(id);
        return NoContent();
    }

    /// <summary>Sube una imagen y la guarda internamente en wwwroot/images/products.</summary>
    [HttpPost("{id:guid}/image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("No se recibió ningún archivo.");

        if (file.Length > TamanoMaximoBytes)
            throw new ArgumentException("La imagen supera el tamaño máximo permitido (5 MB).");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(ext))
            throw new ArgumentException($"Formato no permitido. Use: {string.Join(", ", ExtensionesPermitidas)}.");

        // Verifica que el producto exista antes de guardar el archivo.
        if (await _service.ObtenerProductoPorIdAsync(id) is null)
            return NotFound();

        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var carpeta = Path.Combine(webRoot, "images", "products");
        Directory.CreateDirectory(carpeta);

        var nombreArchivo = $"{Guid.NewGuid():N}{ext}";
        var rutaFisica = Path.Combine(carpeta, nombreArchivo);

        await using (var stream = System.IO.File.Create(rutaFisica))
            await file.CopyToAsync(stream);

        var rutaInterna = $"/images/products/{nombreArchivo}";
        var producto = await _service.ActualizarImagenProductoAsync(id, rutaInterna);
        return Ok(producto);
    }
}
