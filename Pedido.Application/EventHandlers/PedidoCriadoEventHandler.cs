using MediatR;
using Microsoft.Extensions.Logging;
using Pedido.Application.Events;
using Pedido.Application.Interfaces.Integrations.PedidoDestino;

namespace Pedido.Application.EventHandlers
{
    public class PedidoCriadoEventHandler : INotificationHandler<PedidoCriadoEvent>
    {
        private readonly IPedidoDestinoService _pedidoDestinoService;
        private readonly ILogger<PedidoCriadoEventHandler> _logger;

        public PedidoCriadoEventHandler(IPedidoDestinoService pedidoDestinoService, ILogger<PedidoCriadoEventHandler> logger)
        {
            _pedidoDestinoService = pedidoDestinoService ?? throw new ArgumentNullException(nameof(pedidoDestinoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(PedidoCriadoEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                await _pedidoDestinoService.EnviarPedidoAsync(notification.Pedido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar pedido {PedidoId} para o sistema de destino.", notification.Pedido.PedidoId);
            }

        }
    }
}
