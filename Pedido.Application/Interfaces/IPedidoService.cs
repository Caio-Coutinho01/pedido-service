using Pedido.Application.DTOs;
using Pedido.Domain.Enums;

namespace Pedido.Application.Interfaces
{
    public interface IPedidoService
    {
        Task<CriarPedidoResponseDTO> CriarPedidoAsync(CriarPedidoRequestDTO request);
        Task<ConsultarPedidoResponseDTO?> ConsultarPedidoPorIdAsync(int id);
        Task<List<ConsultarPedidoResponseDTO>> ListarPedidosPorStatusAsync(PedidoStatus status);
    }
}
