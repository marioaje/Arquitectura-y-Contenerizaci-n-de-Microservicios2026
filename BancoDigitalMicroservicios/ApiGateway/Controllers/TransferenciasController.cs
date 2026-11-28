using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using SharedContracts;
namespace ApiGateway.Controllers;

[ApiController]
[Route("api/transferencias")]
public sealed class TransferenciasController(IHttpClientFactory factory, ILogger<TransferenciasController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] TransferenciaRequest solicitud, CancellationToken ct)
    {
        var client = factory.CreateClient("TransferenciasApi");
        try
        {
            using var response = await client.PostAsJsonAsync(
                "api/procesartransferencias/process", solicitud, ct);
            var contenido = await response.Content.ReadAsStringAsync(ct);
            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                Content = contenido,
                ContentType = "application/json"
            };
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("Tiempo agotado al contactar TransferenciasApi");
            return StatusCode(504, new { mensaje = "Tiempo agotado; consultar operación antes de reintentar" });
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "TransferenciasApi no disponible");
            return StatusCode(503, new { mensaje = "TransferenciasApi no disponible" });
        }
    }

    [HttpGet]
    public Task<IActionResult> Listar(CancellationToken ct) => Consultar("api/procesartransferencias", ct);

    [HttpGet("{solicitudId:guid}")]
    public Task<IActionResult> Obtener(Guid solicitudId, CancellationToken ct) =>
        Consultar($"api/procesartransferencias/{solicitudId}", ct);

    private async Task<IActionResult> Consultar(string ruta, CancellationToken ct)
    {
        try
        {
            var client = factory.CreateClient("TransferenciasApi");
            using var response = await client.GetAsync(ruta, ct);
            var contenido = await response.Content.ReadAsStringAsync(ct);
            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                Content = contenido,
                ContentType = "application/json"
            };
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return StatusCode(504, new { mensaje = "Tiempo de espera agotado" });
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, new { mensaje = "TransferenciasApi no disponible" });
        }
    }
}
