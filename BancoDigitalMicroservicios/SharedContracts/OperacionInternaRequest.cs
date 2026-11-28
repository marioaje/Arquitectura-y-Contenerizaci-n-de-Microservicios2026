namespace SharedContracts;
public record OperacionInternaRequest(Guid SolicitudId,int CuentaOrigenId,int CuentaDestinoId,decimal Monto);
