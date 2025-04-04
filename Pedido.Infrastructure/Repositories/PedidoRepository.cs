using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pedido.Domain.Entities;
using Pedido.Domain.Enums;
using Pedido.Domain.Interfaces;
using Pedido.Infrastructure.Persistence.Context;

namespace Pedido.Infrastructure.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly PedidoDbContext _context;
        private readonly ILogger<PedidoRepository> _logger;

        public PedidoRepository(PedidoDbContext context, ILogger<PedidoRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> PedidoExisteAsync(int pedidoId)
        {
            return await _context.Pedidos.AnyAsync(p => p.PedidoId == pedidoId);
        }

        public async Task AdicionarAsync(PedidoEntity pedido)
        {
            await _context.Pedidos.AddAsync(pedido);
        }

        public async Task<PedidoEntity?> ObterPorIdAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<PedidoEntity>> ObterPedidosPorStatusAsync(PedidoStatus status)
        {
            return await _context.Pedidos
                .Include(p => p.Itens)
                .Where(p => p.Status == status)
                .ToListAsync();
        }

        public async Task SalvarAlteracoesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao salvar alterações no banco.");
                throw new ApplicationException("Erro ao salvar os dados no banco de dados.", ex);
            }
        }
    }
}
