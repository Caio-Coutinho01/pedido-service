using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pedido.Domain.Interfaces;
using Pedido.Infrastructure.Persistence.Context;
using Pedido.Infrastructure.Repositories;

namespace Pedido.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<PedidoDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IPedidoRepository, PedidoRepository>();

            return services;
        }
    }
}
