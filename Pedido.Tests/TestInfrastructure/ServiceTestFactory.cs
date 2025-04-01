using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using NSubstitute;
using Pedido.Application.Mappings;
using Pedido.Application.Services;
using Pedido.Infrastructure.Persistence.Context;

namespace TestInfrastructure
{
    public static class ServiceTestFactory
    {
        public static ServiceProvider BuildServiceProvider(DbContextOptions<PedidoDbContext> dbContextOptions, bool usarNovaRegraImposto = false)
        {
            var services = new ServiceCollection();

            services.AddScoped(_ => new PedidoDbContext(dbContextOptions));

            var featureManager = Substitute.For<IFeatureManager>();
            featureManager.IsEnabledAsync("UsarNovaRegraImposto").Returns(Task.FromResult(usarNovaRegraImposto));
            services.AddSingleton(featureManager);

            services.AddSingleton(Substitute.For<ILogger<PedidoService>>());

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PedidoProfile>();
            });
            services.AddSingleton(mapperConfig.CreateMapper());

            services.AddScoped<PedidoService>();

            return services.BuildServiceProvider();
        }
    }
}
