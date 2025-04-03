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
        public static ServiceProvider BuildServiceProvider(
            bool usarNovaRegraImposto = false, 
            IPedidoRepository? pedidoRepositoryMock = null, 
            ILogger<PedidoService>? loggerMock = null, 
            IMapper? mapper = null)
        {
            var services = new ServiceCollection();

            var repository = pedidoRepositoryMock ?? Substitute.For<IPedidoRepository>();
            services.AddSingleton(repository);

            var featureManager = Substitute.For<IFeatureManager>();
            featureManager.IsEnabledAsync(FeatureFlags.UsarNovaRegraImposto).Returns(Task.FromResult(usarNovaRegraImposto));
            services.AddSingleton(featureManager);

            services.AddSingleton(loggerMock ?? Substitute.For<ILogger<PedidoService>>());

            if (mapper != null)
            {
                services.AddSingleton(mapper);
            }
            else
            {
                var mapperConfig = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<PedidoProfile>();
                });
                services.AddSingleton(mapperConfig.CreateMapper());
            }

            services.AddScoped<PedidoService>();

            return services.BuildServiceProvider();
        }
    }
}
