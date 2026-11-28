namespace SharedContracts;
public record CuentaDto(int IdCuenta,string NumeroCuenta,int IdCliente,string TipoCuenta,string Moneda,decimal Saldo,string Estado);
