using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pedido.Application.Interfaces.Integrations.PedidoDestino;
using Pedido.Application.Interfaces.Integrations.PedidoDistribuidor;
using Pedido.Domain.Interfaces;
using Pedido.Infrastructure.Integrations.PedidoDestino;
using Pedido.Infrastructure.Integrations.PedidoDistribuidor;
using Pedido.Infrastructure.Persistence.Context;
using Pedido.Infrastructure.Policies;
using Pedido.Infrastructure.Repositories;

namespace Pedido.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<PedidoDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IPedidoRepository, PedidoRepository>();
            services.AddScoped<IPedidoDestinoService, PedidoDestinoService>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<PedidoDestinoService>>();
                var policy = PollyPolicies.WrapResilientPolicy(logger);
                return new PedidoDestinoService(logger, policy);
            });
            services.AddScoped<IPedidoDistribuidorService, PedidoDistribuidorService>();

            return services;
        }
    }
}
