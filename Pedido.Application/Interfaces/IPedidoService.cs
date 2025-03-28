using Pedido.Application.DTOs;

namespace Pedido.Application.Interfaces
{
    public interface IPedidoService
    {
        Task<CriarPedidoResponseDTO> CriarPedidoAsync(CriarPedidoRequestDTO request);
        Task<ConsultarPedidoResponseDTO?> ConsultarPedidoPorIdAsync(int id);
    }
}
