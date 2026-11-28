using System.Net;
using System.Net.Http.Json;
using SharedContracts;
using TransferenciasApi.Messaging;
namespace TransferenciasApi.Services;

public sealed class TransferenciasService(IHttpClientFactory factory, ILogger<TransferenciasService> logger, RabbitMqPublisher publisher)
{
    private readonly object _sync = new();
    private readonly Dictionary<Guid, (TransferenciaRequest Request, TransferenciaResponse Response)> _transferencias = new();
    private int _secuencia;

    public List<TransferenciaResponse> Listar()
    {
        lock (_sync) return _transferencias.Values.Select(x => x.Response).OrderBy(x => x.IdTransferencia).ToList();
    }

    public TransferenciaResponse? Buscar(Guid id)
    {
        lock (_sync) return _transferencias.TryGetValue(id, out var op) ? op.Response : null;
    }

    public async Task<(int Status, object Result)> Procesar(TransferenciaRequest req, CancellationToken ct)
    {
        if (req.SolicitudId == Guid.Empty || req.Monto <= 0 ||
            req.CuentaOrigenId <= 0 || req.CuentaDestinoId <= 0 ||
            req.CuentaOrigenId == req.CuentaDestinoId)
            return (400, new { mensaje = "Solicitud inválida" });

        lock (_sync)
        {
            if (_transferencias.TryGetValue(req.SolicitudId, out var anterior))
                return Coincide(anterior.Request, req)
                    ? (200, anterior.Response)
                    : (409, new { mensaje = "SolicitudId reutilizado con datos diferentes" });
        }

        var client = factory.CreateClient("CuentasApi");
        try
        {
            var solicitudInterna = new OperacionInternaRequest(
                req.SolicitudId, req.CuentaOrigenId, req.CuentaDestinoId, req.Monto);
            using var response = await client.PostAsJsonAsync(
                "api/cuentas/transferencia-interna", solicitudInterna, ct);

            if (!response.IsSuccessStatusCode)
            {
                // Solo devolvemos mensajes de negocio conocidos; no filtramos detalles internos.
                var mensaje = response.StatusCode switch
                {
                    HttpStatusCode.BadRequest => "Solicitud inválida",
                    HttpStatusCode.NotFound => "Cuenta inexistente",
                    HttpStatusCode.Conflict => "La transferencia no cumple las reglas de negocio",
                    _ => "Error al consultar CuentasApi"
                };
                var codigo = response.StatusCode is HttpStatusCode.BadRequest or
                    HttpStatusCode.NotFound or HttpStatusCode.Conflict
                    ? (int)response.StatusCode : 502;
                return (codigo, new { mensaje });
            }

            var operacion = await response.Content.ReadFromJsonAsync<OperacionInternaResponse>(cancellationToken: ct);
            if (operacion is null || operacion.SolicitudId != req.SolicitudId ||
                operacion.CuentaOrigenId != req.CuentaOrigenId ||
                operacion.CuentaDestinoId != req.CuentaDestinoId || operacion.Monto != req.Monto)
                return (502, new { mensaje = "Respuesta inválida de CuentasApi; consultar antes de reintentar" });

            TransferenciaResponse resultado;
            lock (_sync)
            {
                if (_transferencias.TryGetValue(req.SolicitudId, out var anterior))
                    return Coincide(anterior.Request, req)
                        ? (200, anterior.Response)
                        : (409, new { mensaje = "SolicitudId reutilizado con datos diferentes" });

                resultado = new TransferenciaResponse(
                    ++_secuencia, req.SolicitudId, req.CuentaOrigenId, req.CuentaDestinoId,
                    req.Monto, operacion.Moneda, "Completada", "Transferencia realizada correctamente");
                _transferencias.Add(req.SolicitudId, (Copiar(req), resultado));
                logger.LogInformation("Transferencia registrada: {SolicitudId}", req.SolicitudId);
            }
            // Publicar solo una vez, al registrar por primera vez la transferencia.
            var evento = new TransferenciaRealizadaEvent(
                Guid.NewGuid(), "TransferenciaRealizada", resultado.IdTransferencia,
                resultado.CuentaOrigenId, resultado.CuentaDestinoId, resultado.Monto,
                resultado.Moneda, DateTimeOffset.UtcNow);
            try
            {
                await publisher.PublicarAsync(evento, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // La operación bancaria ya se realizó; no debe repetirse el débito.
                logger.LogError(ex, "Transferencia {SolicitudId} completada, pero no se publicó el evento", req.SolicitudId);
            }
            return (200, resultado);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("Timeout en CuentasApi para {SolicitudId}", req.SolicitudId);
            return (504, new { mensaje = "Tiempo de espera agotado; consultar operación antes de reintentar" });
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Error de conexión a CuentasApi");
            return (503, new { mensaje = "CuentasApi no disponible; consultar operación antes de reintentar" });
        }
        catch (System.Text.Json.JsonException ex)
        {
            logger.LogWarning(ex, "Respuesta JSON inválida de CuentasApi");
            return (502, new { mensaje = "Respuesta inválida de CuentasApi; consultar operación antes de reintentar" });
        }
    }

    private static bool Coincide(TransferenciaRequest a, TransferenciaRequest b) =>
        a.CuentaOrigenId == b.CuentaOrigenId && a.CuentaDestinoId == b.CuentaDestinoId &&
        a.Monto == b.Monto && a.Descripcion == b.Descripcion;

    private static TransferenciaRequest Copiar(TransferenciaRequest r) => new()
    {
        SolicitudId = r.SolicitudId, CuentaOrigenId = r.CuentaOrigenId,
        CuentaDestinoId = r.CuentaDestinoId, Monto = r.Monto, Descripcion = r.Descripcion
    };
}
