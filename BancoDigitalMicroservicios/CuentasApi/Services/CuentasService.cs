using SharedContracts;
namespace CuentasApi.Services;

// Datos de demostración: no persisten entre reinicios ni se comparten entre réplicas.
public sealed class CuentasService
{
    private readonly object _sync = new();
    private readonly Dictionary<int, CuentaDto> _cuentas = new()
    {
        [1] = new(1, "CR001", 1, "Ahorros", "CRC", 100000m, "Activa"),
        [2] = new(2, "CR002", 2, "Ahorros", "CRC", 50000m, "Activa"),
        [3] = new(3, "US003", 3, "Corriente", "USD", 500m, "Activa")
    };
    private readonly Dictionary<Guid, (OperacionInternaRequest Request, OperacionInternaResponse Result)> _operaciones = new();

    public List<CuentaDto> Listar()
    {
        lock (_sync) return _cuentas.Values.OrderBy(c => c.IdCuenta).ToList();
    }

    public CuentaDto? Obtener(int id)
    {
        lock (_sync) return _cuentas.GetValueOrDefault(id);
    }

    public OperacionInternaResponse? BuscarOperacion(Guid id)
    {
        lock (_sync) return _operaciones.TryGetValue(id, out var op) ? op.Result : null;
    }

    public (int Status, object Result) Transferir(OperacionInternaRequest req)
    {
        lock (_sync)
        {
            if (req.SolicitudId == Guid.Empty || req.Monto <= 0 ||
                req.CuentaOrigenId <= 0 || req.CuentaDestinoId <= 0 ||
                req.CuentaOrigenId == req.CuentaDestinoId)
                return (400, new { mensaje = "Solicitud inválida" });

            if (_operaciones.TryGetValue(req.SolicitudId, out var anterior))
            {
                if (anterior.Request != req)
                    return (409, new { mensaje = "SolicitudId reutilizado con datos diferentes" });
                return (200, anterior.Result);
            }

            if (!_cuentas.TryGetValue(req.CuentaOrigenId, out var origen) ||
                !_cuentas.TryGetValue(req.CuentaDestinoId, out var destino))
                return (404, new { mensaje = "Cuenta inexistente" });

            if (origen.Estado != "Activa" || destino.Estado != "Activa")
                return (409, new { mensaje = "Cuenta inactiva" });
            if (origen.Moneda != destino.Moneda)
                return (409, new { mensaje = "Las monedas son diferentes" });
            if (origen.Saldo < req.Monto)
                return (409, new { mensaje = "Fondos insuficientes" });

            // Ambas modificaciones ocurren dentro del mismo lock, en este único proceso.
            _cuentas[origen.IdCuenta] = origen with { Saldo = origen.Saldo - req.Monto };
            _cuentas[destino.IdCuenta] = destino with { Saldo = destino.Saldo + req.Monto };

            var resultado = new OperacionInternaResponse(
                req.SolicitudId, req.CuentaOrigenId, req.CuentaDestinoId, req.Monto, origen.Moneda);
            _operaciones.Add(req.SolicitudId, (req, resultado));
            return (200, resultado);
        }
    }
}
