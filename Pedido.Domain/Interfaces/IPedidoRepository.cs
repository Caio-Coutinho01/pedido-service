using Pedido.Domain.Entities;
using Pedido.Domain.Enums;

namespace Pedido.Domain.Interfaces
{
    public interface IPedidoRepository
    {
        Task<bool> PedidoExisteAsync(int pedidoId);
        Task AdicionarAsync(PedidoEntity pedido);
        Task<PedidoEntity?> ObterPorIdAsync(int PedidoId);
        Task<List<PedidoEntity>> ObterPedidosPorStatusAsync(PedidoStatus status);
        Task SalvarAlteracoesAsync();
        Task<List<PedidoEntity>> ObterPedidosElegiveisParaEnvioAsync(int maxTentativas);
    }
}
