using HungryUpBackend.Modules.Billing.Entities;
using HungryUpBackend.Modules.Billing.Services;
using Microsoft.AspNetCore.Mvc;

namespace HungryUpBackend.Modules.Billing.Controllers;

[ApiController]
[Route("api/v1/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _service;

    public BillingController(IBillingService service) => _service = service;

    [HttpPost("pay")]
    public async Task<IActionResult> Pay([FromBody] PagarRequest request)
    {
        var pago = await _service.ProcesarPagoAsync(
            request.PedidoId, request.MetodoPago, request.MontoPagado);
        return Ok(pago);
    }
}

public record PagarRequest(Guid PedidoId, MetodoPago MetodoPago, decimal MontoPagado);
