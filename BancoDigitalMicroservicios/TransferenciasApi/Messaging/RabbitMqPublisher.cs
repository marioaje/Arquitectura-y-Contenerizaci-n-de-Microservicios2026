
using System.Text.Json;      // Permite convertir objetos C# a formato JSON.
using RabbitMQ.Client;       // Biblioteca para conectarse y publicar mensajes en RabbitMQ.
using SharedContracts;       // Contiene los modelos compartidos entre microservicios.

namespace TransferenciasApi.Messaging;

/// <summary>
/// Servicio encargado de publicar eventos de transferencias bancarias
/// en RabbitMQ utilizando comunicación asíncrona.
///
/// Este servicio actúa como PRODUCTOR (Publisher).
/// </summary>
public sealed class RabbitMqPublisher(
    IConfiguration config,
    ILogger<RabbitMqPublisher> logger)
{
    /// <summary>
    /// Publica un evento cuando se realiza una transferencia bancaria.
    /// </summary>
    /// <param name="evento">
    /// Objeto que contiene la información de la transferencia realizada.
    /// </param>
    /// <param name="ct">
    /// Token que permite cancelar la operación asíncrona.
    /// </param>
    public async Task PublicarAsync(
        TransferenciaRealizadaEvent evento,
        CancellationToken ct)
    {
        // PASO 1: CONFIGURAR LA CONEXIÓN CON RABBITMQ
        // Se leen los valores desde appsettings.json.
        // Si no existen, se utilizan los valores predeterminados.

        var factory = new ConnectionFactory
        {
            // Dirección del servidor RabbitMQ.
            HostName = config["RabbitMq:Host"] ?? "localhost",

            // Usuario para autenticarse en RabbitMQ.
            UserName = config["RabbitMq:User"] ?? "guest",

            // Contraseña del usuario RabbitMQ.
            Password = config["RabbitMq:Password"] ?? "guest"
        };

        // PASO 2: ESTABLECER LA CONEXIÓN
        // CreateConnectionAsync abre una conexión TCP con RabbitMQ.
        // await using libera la conexión al finalizar el método.

        await using var connection =
            await factory.CreateConnectionAsync(ct);

        // PASO 3: CREAR UN CANAL
        // El canal permite realizar operaciones con RabbitMQ,
        // como declarar colas y publicar mensajes.

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: ct);

        // PASO 4: DECLARAR LA COLA
        // Si la cola no existe, RabbitMQ la crea.
        // Si ya existe con la misma configuración, la reutiliza.

        await channel.QueueDeclareAsync(
            queue: "transferencias.realizadas",

            // La cola permanece declarada después de reiniciar RabbitMQ.
            durable: true,

            // Permite que diferentes conexiones utilicen la cola.
            exclusive: false,

            // La cola no se elimina automáticamente.
            autoDelete: false,

            // No se utilizan argumentos adicionales.
            arguments: null,

            // Permite cancelar la operación.
            cancellationToken: ct
        );

        // PASO 5: SERIALIZAR EL EVENTO
        // Convierte el objeto TransferenciaRealizadaEvent
        // en un JSON representado como arreglo de bytes UTF-8.
        //
        // RabbitMQ transporta mensajes como secuencias de bytes.

        var body = JsonSerializer.SerializeToUtf8Bytes(evento);

        // PASO 6: CONFIGURAR LAS PROPIEDADES DEL MENSAJE

        var properties = new BasicProperties
        {
            // Solicita que el mensaje sea persistente.
            // Junto con una cola durable, favorece su recuperación
            // después de un reinicio del servidor.
            Persistent = true,

            // Indica que el contenido del mensaje es JSON.
            ContentType = "application/json"
        };

        // PASO 7: PUBLICAR EL MENSAJE EN RABBITMQ
        // Se envía el evento a la cola transferencias.realizadas.

        await channel.BasicPublishAsync(

            // Exchange predeterminado de RabbitMQ.
            exchange: "",

            // Nombre de la cola que recibirá el mensaje.
            routingKey: "transferencias.realizadas",

            // No exige notificación si el mensaje no puede enrutarse.
            mandatory: false,

            // Propiedades configuradas anteriormente.
            basicProperties: properties,

            // Contenido JSON convertido a bytes.
            body: body,

            // Token de cancelación.
            cancellationToken: ct
        );

        // PASO 8: REGISTRAR LA PUBLICACIÓN
        // Escribe un mensaje informativo en el log de la API.
        // EventId permite identificar el evento publicado.

        logger.LogInformation(
            "Evento publicado en RabbitMQ: {EventId}",
            evento.EventId
        );
    }
}
