using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Pedido.Application.Configuration;
using Pedido.Domain.Enums;
using Pedido.Infrastructure.DependencyInjection;
using Serilog;
using System.Reflection;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

#region Serilog
// Configura��o do Serilog
builder.Host.UseSerilog((context, loggerConfig) =>
{
    try
    {
        loggerConfig
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.MSSqlServer(
                connectionString: context.Configuration.GetConnectionString("DefaultConnection"),
                sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
                {
                    TableName = "Logs",
                    AutoCreateSqlTable = true
                },
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error
            );
    }
    catch (Exception ex)
    {
        Console.WriteLine("Serilog falhou durante startup (provavelmente execu��o de migrations). Erro: " + ex.Message);
    }
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pedido.API", Version = "v1" });
    c.MapType<PedidoStatus>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(PedidoStatus)).Select(n => new OpenApiString(n)).Cast<IOpenApiAny>().ToList()
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.EnableAnnotations();
});

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddInfrastructure(connectionString).AddApplication();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Pedido.Application.AssemblyReference).Assembly));

// HangFire
builder.Services.AddHangfire(config =>
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddHangfireServer();

builder.Services.Configure<EnvioPedidosOptions>(builder.Configuration.GetSection("EnvioPedidos"));

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

app.UseRouting();

app.UseHangfireDashboard();
Pedido.API.BackgroundJobs.JobScheduler.ConfigurarJobs();

#endregion

app.Run();
