using AutoMapper;
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
        private readonly IMapper _mapper;

        public PedidoService(PedidoDbContext context, IFeatureManager featureManager, ILogger<PedidoService> logger, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<CriarPedidoResponseDTO> CriarPedidoAsync(CriarPedidoRequestDTO requestDto)
        {
            try
            {
                var pedidoDuplicado = await _context.Pedidos
                    .AnyAsync(p => p.PedidoId == requestDto.PedidoId);

                if (pedidoDuplicado)
                    throw new InvalidOperationException("Este pedido já existe.");

                var pedido = _mapper.Map<PedidoEntity>(requestDto);
                pedido.Status = PedidoStatus.Criado;

                var usarNovaRegra = await _featureManager.IsEnabledAsync("usarNovaRegraImposto");
                pedido.CalcularImposto(usarNovaRegra);

                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                return _mapper.Map<CriarPedidoResponseDTO>(pedido);

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

                return _mapper.Map<ConsultarPedidoResponseDTO>(pedido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar o pedido com Id: {Id}", id);
                throw new ApplicationException($"Erro ao consultar o pedido {id}.", ex);
            }
        }


        public async Task<List<ConsultarPedidoResponseDTO>> ListarPedidosPorStatusAsync(PedidoStatus status)
        {
            try
            {
                var pedidos = await _context.Pedidos
                    .Include(x => x.Itens)
                    .Where(x => x.Status == status)
                    .ToListAsync();

                return _mapper.Map<List<ConsultarPedidoResponseDTO>>(pedidos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar pedidos por status: {Status}", status);
                throw new ApplicationException($"Erro ao listar pedidos por status {status}.", ex);
            }
        }
    }
}
