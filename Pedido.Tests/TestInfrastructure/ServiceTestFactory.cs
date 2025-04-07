using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using NSubstitute;
using Pedido.Application.Configuration;
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
            IMapper? mapper = null,
            IOptionsMonitor<EnvioPedidosOptions>? options = null)
        {
            var services = new ServiceCollection();

            var repository = pedidoRepositoryMock ?? Substitute.For<IPedidoRepository>();
            services.AddSingleton(repository);

            var featureManager = Substitute.For<IFeatureManager>();
            featureManager.IsEnabledAsync(FeatureFlags.UsarNovaRegraImposto).Returns(Task.FromResult(usarNovaRegraImposto));
            services.AddSingleton(featureManager);

            services.AddSingleton(loggerMock ?? Substitute.For<ILogger<PedidoService>>());

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PedidoProfile>();
            });
            services.AddSingleton<IMapper>(mapper ?? mapperConfig.CreateMapper());

            if (options != null)
            {
                services.AddSingleton(options);
            }
            else
            {
                var optionsMock = Substitute.For<IOptionsMonitor<EnvioPedidosOptions>>();
                optionsMock.CurrentValue.Returns(new EnvioPedidosOptions { MaxTentativas = 3 });
                services.AddSingleton(optionsMock);
            }

            services.AddSingleton<Pedido.Application.Interfaces.Integrations.PedidoDestino.IPedidoDestinoService>
                (Substitute.For<Pedido.Application.Interfaces.Integrations.PedidoDestino.IPedidoDestinoService>());

            services.AddSingleton<MediatR.IMediator>(Substitute.For<MediatR.IMediator>());

            services.AddScoped<PedidoService>();

            return services.BuildServiceProvider();
        }
    }
}
