using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HungryUpBackend;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, title) = ex switch
        {
            KeyNotFoundException    => (StatusCodes.Status404NotFound,            "Recurso no encontrado"),
            InvalidOperationException => (StatusCodes.Status400BadRequest,        "Operación inválida"),
            ArgumentException       => (StatusCodes.Status400BadRequest,          "Parámetro inválido"),
            _                       => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title  = title,
            Detail = ex.Message
        }, ct);

        return true;
    }
}
