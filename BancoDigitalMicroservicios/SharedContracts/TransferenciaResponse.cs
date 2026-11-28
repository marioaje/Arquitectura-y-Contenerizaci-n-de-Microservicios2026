namespace SharedContracts;
public record TransferenciaResponse(int IdTransferencia,Guid SolicitudId,int CuentaOrigenId,int CuentaDestinoId,decimal Monto,string Moneda,string Estado,string Mensaje);
