using Microsoft.Extensions.Logging;
using Pedido.Application.DTOs.Response;
using Pedido.Application.Interfaces.Integrations.PedidoDestino;
using Polly;

namespace Pedido.Infrastructure.Integrations.PedidoDestino
{
    public class PedidoDestinoService : IPedidoDestinoService
    {
        private readonly ILogger<PedidoDestinoService> _logger;
        private readonly IAsyncPolicy _policy;

        public PedidoDestinoService(ILogger<PedidoDestinoService> logger, IAsyncPolicy policy)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        public async Task EnviarPedidoAsync(ConsultarPedidoResponseDTO pedido)
        {
            await _policy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"Enviando pedido {pedido.PedidoId} para o sistema B...");

                if (new Random().Next(1, 4) == 1)
                    throw new Exception("Erro simulado no envio");
            });
        }
    }
}

