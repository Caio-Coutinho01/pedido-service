using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Pedido.Application.Interfaces;
using Pedido.Application.Mappings;
using Pedido.Application.Services;

namespace Pedido.Application.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        services.AddScoped<IPedidoService, PedidoService>();
        services.AddAutoMapper(typeof(PedidoProfile));
        services.AddFeatureManagement();

        return services;
    }
}
