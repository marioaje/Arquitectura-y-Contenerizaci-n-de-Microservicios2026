using TransferenciasApi.Messaging;
using TransferenciasApi.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<TransferenciasService>();
// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddHttpClient("CuentasApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CuentasApi"]
        ?? throw new InvalidOperationException("Falta Services:CuentasApi"));
    client.Timeout = TimeSpan.FromSeconds(5);
});
var app = builder.Build();
// Habilitar Swagger en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
