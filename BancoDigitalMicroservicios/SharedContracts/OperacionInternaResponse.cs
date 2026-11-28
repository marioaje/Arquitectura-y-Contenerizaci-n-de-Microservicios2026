namespace SharedContracts;
public record OperacionInternaResponse(Guid SolicitudId,int CuentaOrigenId,int CuentaDestinoId,decimal Monto,string Moneda);
