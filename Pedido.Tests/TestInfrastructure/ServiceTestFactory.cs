using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using NSubstitute;
using Pedido.Application.Mappings;
using Pedido.Application.Services;
using Pedido.Domain.Constants;
using Pedido.Domain.Interfaces;

namespace TestInfrastructure
{
    public static class ServiceTestFactory
    {
        public static ServiceProvider BuildServiceProvider(bool usarNovaRegraImposto = false)
        {
            var services = new ServiceCollection();

            var pedidoRepository = Substitute.For<IPedidoRepository>();
            services.AddSingleton(pedidoRepository);

            var featureManager = Substitute.For<IFeatureManager>();
            featureManager.IsEnabledAsync(FeatureFlags.UsarNovaRegraImposto).Returns(Task.FromResult(usarNovaRegraImposto));
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
