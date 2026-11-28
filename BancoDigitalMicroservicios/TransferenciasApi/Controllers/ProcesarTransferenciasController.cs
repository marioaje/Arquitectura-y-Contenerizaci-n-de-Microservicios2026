using Microsoft.AspNetCore.Mvc;
using SharedContracts;
using TransferenciasApi.Services;
namespace TransferenciasApi.Controllers;

[ApiController]
[Route("api/procesartransferencias")]
public sealed class ProcesarTransferenciasController(TransferenciasService service) : ControllerBase
{
    [HttpPost("process")]
    public async Task<IActionResult> Procesar([FromBody] TransferenciaRequest solicitud, CancellationToken ct)
    {
        var (status, resultado) = await service.Procesar(solicitud, ct);
        return StatusCode(status, resultado);
    }

    [HttpGet]
    public IActionResult Listar() => Ok(service.Listar());

    [HttpGet("{solicitudId:guid}")]
    public IActionResult Obtener(Guid solicitudId) => service.Buscar(solicitudId) is { } resultado
        ? Ok(resultado) : NotFound(new { mensaje = "Transferencia no encontrada" });
}
