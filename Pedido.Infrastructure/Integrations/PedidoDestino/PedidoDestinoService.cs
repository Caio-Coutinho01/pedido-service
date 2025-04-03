using Microsoft.Extensions.Logging;
using Pedido.Application.DTOs.Response;
using Pedido.Application.Interfaces.Integrations.PedidoDestino;

namespace Pedido.Infrastructure.Integrations.PedidoDestino
{
    public class PedidoDestinoService : IPedidoDestinoService
    {
        private readonly ILogger<PedidoDestinoService> _logger;

        public PedidoDestinoService(ILogger<PedidoDestinoService> logger)
        {
            _logger = logger;
        }

        public async Task EnviarPedidoAsync(ConsultarPedidoResponseDTO pedido)
        {
            _logger.LogInformation("Pedido {PedidoId} enviado ao sistema de destino.", pedido.PedidoId);
            await Task.CompletedTask;
        }
    }
}

