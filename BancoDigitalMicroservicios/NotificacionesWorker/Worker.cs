using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedContracts;

namespace NotificacionesWorker;

public sealed class Worker(IConfiguration config, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = config["RabbitMq:Host"] ?? "localhost",
                    UserName = config["RabbitMq:User"] ?? "guest",
                    Password = config["RabbitMq:Password"] ?? "guest"
                };
                await using var connection = await factory.CreateConnectionAsync(stoppingToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                await channel.QueueDeclareAsync("transferencias.realizadas", durable: true,
                    exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
                await channel.BasicQosAsync(0, 1, false, stoppingToken);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var evento = JsonSerializer.Deserialize<TransferenciaRealizadaEvent>(ea.Body.Span);
                        if (evento is null) throw new JsonException("Evento vacío");
                        logger.LogInformation("NOTIFICACIÓN: transferencia #{Id}, monto {Monto} {Moneda}, origen {Origen}, destino {Destino}",
                            evento.IdTransferencia, evento.Monto, evento.Moneda, evento.CuentaOrigenId, evento.CuentaDestinoId);
                        await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    }
                    catch (JsonException ex)
                    {
                        logger.LogError(ex, "Evento inválido: se descarta para evitar reintentos infinitos");
                        await channel.BasicRejectAsync(ea.DeliveryTag, requeue: false, cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error procesando notificación");
                        await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true, cancellationToken: stoppingToken);
                    }
                };
                await channel.BasicConsumeAsync("transferencias.realizadas", autoAck: false,
                    consumer: consumer, cancellationToken: stoppingToken);
                logger.LogInformation("Esperando mensajes de RabbitMQ...");
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "RabbitMQ no disponible; reintentando en 5 segundos");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
