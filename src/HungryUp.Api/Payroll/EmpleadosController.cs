using HungryUp.Api.Authorization;
using HungryUp.Application.Auth;
using HungryUp.Application.Payroll;
using HungryUp.Application.Payroll.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HungryUp.Api.Payroll;

[ApiController]
[Route("api/v1/empleados")]
[Authorize]
public class EmpleadosController : ControllerBase
{
    private readonly IPayrollService _service;

    public EmpleadosController(IPayrollService service) => _service = service;

    [HttpGet]
    [HasPermission(Permissions.Employees.Read)]
    public async Task<IActionResult> GetEmpleados([FromQuery] bool activos = false) =>
        Ok(await _service.GetEmpleadosAsync(activos));

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Employees.Read)]
    public async Task<IActionResult> GetEmpleado(Guid id)
    {
        var empleado = await _service.ObtenerPorIdAsync(id);
        return empleado is null ? NotFound() : Ok(empleado);
    }

    [HttpPost]
    [HasPermission(Permissions.Employees.Create)]
    public async Task<IActionResult> CrearEmpleado([FromBody] CreateEmpleadoDto dto)
    {
        var empleado = await _service.CrearAsync(dto);
        return CreatedAtAction(nameof(GetEmpleado), new { id = empleado.Id }, empleado);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Employees.Update)]
    public async Task<IActionResult> ActualizarEmpleado(Guid id, [FromBody] UpdateEmpleadoDto dto) =>
        Ok(await _service.ActualizarAsync(id, dto));

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Employees.Delete)]
    public async Task<IActionResult> EliminarEmpleado(Guid id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
