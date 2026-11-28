namespace SharedContracts;
public record TransferenciaRealizadaEvent(Guid EventId,string EventType,int IdTransferencia,int CuentaOrigenId,int CuentaDestinoId,decimal Monto,string Moneda,DateTimeOffset Fecha);
