using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Pedido.Application.DTOs;
using Pedido.Application.Interfaces;
using Pedido.Domain.Entities;
using Pedido.Domain.Enums;
using Pedido.Infrastructure.Persistence.Context;

namespace Pedido.Application.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly PedidoDbContext _context;
        private readonly IFeatureManager _featureManager;
        private readonly ILogger<PedidoService> _logger;

        public PedidoService(PedidoDbContext context, IFeatureManager featureManager, ILogger<PedidoService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CriarPedidoResponseDTO> CriarPedidoAsync(CriarPedidoRequestDTO requestDto)
        {
            try
            {
                var pedidoDuplicado = await _context.Pedidos
                    .AnyAsync(p => p.PedidoId == requestDto.PedidoId);

                if (pedidoDuplicado)
                    throw new InvalidOperationException("Este pedido já existe.");

                var pedido = new PedidoEntity
                {
                    PedidoId = requestDto.PedidoId,
                    ClienteId = requestDto.ClienteId,
                    Status = PedidoStatus.Criado,
                    Itens = requestDto.Itens.Select(i => new PedidoItemEntity
                    {
                        ProdutoId = i.ProdutoId,
                        Quantidade = i.Quantidade,
                        Valor = i.Valor
                    }).ToList()
                };

                var usarNovaRegra = await _featureManager.IsEnabledAsync("usarNovaRegraImposto");
                pedido.CalcularImposto(usarNovaRegra);

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                return new CriarPedidoResponseDTO
                {
                    Id = pedido.Id,
                    Status = pedido.Status.ToString()
                };


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar o pedido com Id: {Id}", requestDto.PedidoId);
                throw new ApplicationException($"Erro ao criar o pedido {requestDto.PedidoId}.", ex);
            }
        }

        public async Task<ConsultarPedidoResponseDTO?> ConsultarPedidoPorIdAsync(int id)
        {
            try
            {
                var pedido = await _context.Pedidos
                    .Include(x => x.Itens)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (pedido == null)
                    return null;

                return new ConsultarPedidoResponseDTO
                {
                    Id = pedido.Id,
                    PedidoId = pedido.PedidoId,
                    ClienteId = pedido.ClienteId,
                    Imposto = pedido.Imposto,
                    Status = pedido.Status.ToString(),
                    Itens = pedido.Itens.Select(i => new ItemPedidoDTO
                    {
                        ProdutoId = i.ProdutoId,
                        Quantidade = i.Quantidade,
                        Valor = i.Valor
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar o pedido com Id: {Id}", id);
                throw new ApplicationException($"Erro ao consultar o pedido {id}.", ex);
            }
        }
    }
}
