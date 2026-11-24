using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(
    options =>
    {
        options.AddPolicy("PoliticaBanco", policy =>{
            policy.WithOrigins(
                "http://localhost:4200",
                "https://bccr.fi.cr")
                .AllowAnyHeader()
                .AllowAnyMethod();
    
    });
});


builder.Services.AddHealthChecks();

//Construccion del app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "API Banco v1"
        );
    });
}

app.UseHttpsRedirection();

app.UseCors("PoliticaBanco");

app.UseAuthorization();

app.MapControllers();

//Checkpoint de disponible  Endpoint de is alive
app.MapHealthChecks("/health");

//Inicio del app
app.Run();
