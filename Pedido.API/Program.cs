using Serilog;
using Microsoft.FeatureManagement;
using Microsoft.EntityFrameworkCore;
using Pedido.Infrastructure.Persistence.Context;
using Pedido.Application.Interfaces;
using Pedido.Application.Services;

var builder = WebApplication.CreateBuilder(args);

#region Serilog
// Configuração do Serilog
builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});
#endregion

#region Add Services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Feature Management
builder.Services.AddFeatureManagement();

// Service de pedidos
builder.Services.AddScoped<IPedidoService, PedidoService>();

// DbContext
builder.Services.AddDbContext<PedidoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

var app = builder.Build();

#region Middleware
// Logging
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();
