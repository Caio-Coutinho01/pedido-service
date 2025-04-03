using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Pedido.Application.DTOs.Request;
using Pedido.Application.DTOs.Response;
using Pedido.Application.Interfaces;
using Pedido.Domain.Constants;
using Pedido.Domain.Entities;
using Pedido.Domain.Enums;
using Pedido.Domain.Interfaces;

namespace Pedido.Application.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IFeatureManager _featureManager;
        private readonly ILogger<PedidoService> _logger;
        private readonly IMapper _mapper;

        public PedidoService(IPedidoRepository pedidoRepository, IFeatureManager featureManager, ILogger<PedidoService> logger, IMapper mapper)
        {
            _pedidoRepository = pedidoRepository ?? throw new ArgumentNullException(nameof(pedidoRepository));
            _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<CriarPedidoResponseDTO> CriarPedidoAsync(CriarPedidoRequestDTO requestDto)
        {
            try
            {
                var pedidoDuplicado = await _pedidoRepository.PedidoExisteAsync(requestDto.PedidoId);

                if (pedidoDuplicado)
                    throw new InvalidOperationException("Este pedido já existe.");

                var pedido = _mapper.Map<PedidoEntity>(requestDto);
                pedido.Status = PedidoStatus.Criado;

                var usarNovaRegra = await _featureManager.IsEnabledAsync(FeatureFlags.UsarNovaRegraImposto);
                pedido.CalcularImposto(usarNovaRegra);

                await _pedidoRepository.AdicionarAsync(pedido);
                await _pedidoRepository.SalvarAlteracoesAsync();

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
                var pedido = await _pedidoRepository.ObterPorIdAsync(id);

                if (pedido == null)
                    throw new ApplicationException($"Pedido não encontrado com o ID: {id}");

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
                var pedidos = await _pedidoRepository.ObterPorStatusAsync(status);

                return _mapper.Map<List<ConsultarPedidoResponseDTO>>(pedidos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar pedidos por status: {Status}", status);
                throw new ApplicationException($"Erro ao listar pedidos por status {status}.", ex);
            }
        }

        public async Task<CancelarPedidoResponseDTO> CancelarPedidoAsync(int id, CancelarPedidoRequestDTO dto)
        {
            try
            {
                var pedido = await _pedidoRepository.ObterPorIdAsync(id);

                if (pedido == null)
                    throw new ApplicationException($"Pedido não encontrado com o ID: {id}");

                if (pedido.Status != PedidoStatus.Criado)
                    throw new ApplicationException($"O Pedido {id} não pode ser cancelado, pois já está em processamento ou cancelado!");

                pedido.CancelarPedido(dto.JustificativaCancelamento);
                await _pedidoRepository.SalvarAlteracoesAsync();

                return new CancelarPedidoResponseDTO
                {
                    Sucesso = true,
                    Mensagem = $"Pedido {id} cancelado com sucesso."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar o pedido com Id: {Id}", id);
                throw new ApplicationException($"Erro ao cancelar o pedido {id}.", ex);
            }
        }
    }
}
