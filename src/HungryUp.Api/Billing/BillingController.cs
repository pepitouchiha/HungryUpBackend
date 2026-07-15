using HungryUp.Api.Authorization;
using HungryUp.Application.Auth;
using HungryUp.Application.Billing;
using HungryUp.Application.Billing.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HungryUp.Api.Billing;

[ApiController]
[Route("api/v1/billing")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IBillingService _service;

    public BillingController(IBillingService service) => _service = service;

    [HttpPost("pay")]
    [HasPermission(Permissions.Billing.Pay)]
    public async Task<IActionResult> Pay([FromBody] ProcesarPagoDto dto)
    {
        var pago = await _service.ProcesarPagoAsync(dto);
        return Ok(pago);
    }
}
