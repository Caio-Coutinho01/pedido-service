using MediatR;
using Pedido.Application.DTOs.Response;

namespace Pedido.Application.Events
{
    public class PedidoCriadoEvent : INotification
    {
        public ConsultarPedidoResponseDTO Pedido { get; }

        public PedidoCriadoEvent(ConsultarPedidoResponseDTO pedido)
        {
            Pedido = pedido;
        }
    }
}
