using Pedido.Application.DTOs.Request;
using Pedido.Application.DTOs.Response;
using Pedido.Domain.Enums;

namespace Pedido.Application.Interfaces
{
    public interface IPedidoService
    {
        Task<CriarPedidoResponseDTO> CriarPedidoAsync(CriarPedidoRequestDTO request);
        Task<ConsultarPedidoResponseDTO?> ConsultarPedidoPorIdAsync(int id);
        Task<List<ConsultarPedidoResponseDTO>> ListarPedidosPorStatusAsync(PedidoStatus status);
        Task<CancelarPedidoResponseDTO> CancelarPedidoAsync(int id, CancelarPedidoRequestDTO dto);
    }
}
