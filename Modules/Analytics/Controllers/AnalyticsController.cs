using HungryUpBackend.Modules.Analytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace HungryUpBackend.Modules.Analytics.Controllers;

[ApiController]
[Route("api/v1/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _service;

    public AnalyticsController(IAnalyticsService service) => _service = service;

    [HttpGet("sales-summary")]
    public async Task<IActionResult> GetSalesSummary([FromQuery] string periodo = "dia")
    {
        var result = await _service.ObtenerResumenAsync(periodo);
        return Ok(result);
    }
}
