using Serilog;
using Microsoft.FeatureManagement;
using Microsoft.EntityFrameworkCore;
using Pedido.Infrastructure.Persistence.Context;
using Pedido.Application.Interfaces;
using Pedido.Application.Services;
using Pedido.Application.Mappings;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Pedido.Domain.Enums;


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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Pedido.API", Version = "v1" });
    c.MapType<PedidoStatus>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(PedidoStatus))
            .Select(n => new OpenApiString(n)).Cast<IOpenApiAny>().ToList()
    });
});

// Feature Management
builder.Services.AddFeatureManagement();

// Service de pedidos
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddAutoMapper(typeof(PedidoProfile));

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
