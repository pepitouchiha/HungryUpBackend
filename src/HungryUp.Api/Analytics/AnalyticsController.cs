using HungryUp.Application.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HungryUp.Api.Analytics;

[ApiController]
[Route("api/v1/analytics")]
[Authorize]
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
