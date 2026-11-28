# Banco Digital CR — Microservicios ASP.NET Core 10

## Proyectos
- `ApiGateway`: puerto 5000, entrada pública de transferencias.
- `TransferenciasApi`: puerto 5002, coordinación y registro de transferencias.
- `CuentasApi`: puerto 5001, saldos y aplicación indivisible de operaciones en memoria.
- `SharedContracts`: DTO compartidos.

## Requisitos
Visual Studio Insider con SDK .NET 10, o .NET SDK 10 desde terminal.

## Ejecutar
Abrir `BancoDigitalMicroservicios.sln`. Configurar las tres APIs como proyectos de inicio o usar tres terminales:

```bash
dotnet run --project CuentasApi
dotnet run --project TransferenciasApi
dotnet run --project ApiGateway
```

Los perfiles `launchSettings.json` definen los puertos 5001, 5002 y 5000 respectivamente.

## Pruebas
- `GET http://localhost:5001/api/cuentas`
- `GET http://localhost:5001/api/cuentas/1/saldo`
- `GET http://localhost:5000/health` (también puertos 5001 y 5002)
- `POST http://localhost:5000/api/transferencias` con `Content-Type: application/json`:

```json
{
  "solicitudId": "11111111-1111-4111-8111-111111111111",
  "cuentaOrigenId": 1,
  "cuentaDestinoId": 2,
  "monto": 25000,
  "descripcion": "Pago de servicios"
}
```

- `GET http://localhost:5000/api/transferencias`
- `GET http://localhost:5000/api/transferencias/11111111-1111-4111-8111-111111111111`
- Repetir el POST con el mismo `solicitudId` y datos para comprobar que los saldos no se descuentan dos veces.
- Repetir el mismo `solicitudId` con otro monto para comprobar el conflicto 409 (mientras exista el registro en TransferenciasApi).

## Alcance y advertencias
Ejemplo **exclusivamente académico**. Datos en memoria: al reiniciar se pierden; no usar múltiples réplicas. El endpoint `transferencia-interna` no tiene autenticación entre servicios y no debe exponerse en producción. La consistencia entre el registro de transferencias y los saldos no está garantizada ante reinicios o fallos parciales. Un sistema real requiere persistencia transaccional, autorización, auditoría, recuperación y un diseño robusto de idempotencia.

El contrato `TransferenciaRealizadaEvent` está definido pero **no se publica** en RabbitMQ todavía.
