using CuentasApi.Services;
using Microsoft.AspNetCore.Mvc;
using SharedContracts;
namespace CuentasApi.Controllers;

[ApiController]
[Route("api/cuentas")]
public sealed class CuentasController(CuentasService cuentas) : ControllerBase
{
    [HttpGet]
    public IActionResult Listar() => Ok(cuentas.Listar());

    [HttpGet("{id:int}")]
    public IActionResult Obtener(int id) => cuentas.Obtener(id) is { } cuenta
        ? Ok(cuenta) : NotFound(new { mensaje = "Cuenta no encontrada" });

    [HttpGet("{id:int}/saldo")]
    public IActionResult Saldo(int id) => cuentas.Obtener(id) is { } cuenta
        ? Ok(new { cuenta.IdCuenta, cuenta.Moneda, cuenta.Saldo })
        : NotFound(new { mensaje = "Cuenta no encontrada" });

    // SOLO PARA LABORATORIO: este endpoint requiere autenticación entre servicios en producción.
    [HttpPost("transferencia-interna")]
    public IActionResult Transferir([FromBody] OperacionInternaRequest solicitud)
    {
        var (status, resultado) = cuentas.Transferir(solicitud);
        return StatusCode(status, resultado);
    }

    [HttpGet("operaciones/{id:guid}")]
    public IActionResult Operacion(Guid id) => cuentas.BuscarOperacion(id) is { } resultado
        ? Ok(resultado) : NotFound(new { mensaje = "Operación no encontrada" });
}
