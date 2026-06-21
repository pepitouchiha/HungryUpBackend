using HungryUp.Application.Catalog;
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
    public async Task<IActionResult> GetCategorias() =>
        Ok(await _service.GetCategoriasAsync());
}
