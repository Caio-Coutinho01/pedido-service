using Microsoft.Extensions.Logging;
using Pedido.Application.DTOs.Response;
using Pedido.Application.Interfaces.Integrations.PedidoDistribuidor;

namespace Pedido.Infrastructure.Integrations.PedidoDistribuidor
{
    public class PedidoDistribuidorService : IPedidoDistribuidorService
    {
        private readonly ILogger<PedidoDistribuidorService> _logger;

        public PedidoDistribuidorService(ILogger<PedidoDistribuidorService> logger)
        {
            _logger = logger;
        }

        public async Task DistribuirPedidoAsync(ConsultarPedidoResponseDTO pedido)
        {
            _logger.LogInformation("Distribuindo pedido {PedidoId} para múltiplos destinos...", pedido.PedidoId);
            await Task.CompletedTask;
        }
    }
}
