using Pedido.Domain.Entities;
using Pedido.Domain.Enums;

namespace Pedido.Domain.Interfaces
{
    public interface IPedidoRepository
    {
        Task<bool> PedidoExisteAsync(int pedidoId);
        Task AdicionarAsync(PedidoEntity pedido);
        Task<PedidoEntity?> ObterPorIdAsync(int id);
        Task<List<PedidoEntity>> ObterPorStatusAsync(PedidoStatus status);
        Task SalvarAlteracoesAsync();
    }
}
