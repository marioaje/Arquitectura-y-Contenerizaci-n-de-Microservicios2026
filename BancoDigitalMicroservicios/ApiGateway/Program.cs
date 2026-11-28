var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddHttpClient("TransferenciasApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:TransferenciasApi"]
        ?? throw new InvalidOperationException("Falta Services:TransferenciasApi"));
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
