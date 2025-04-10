﻿using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Pedido.Application.Configuration;
using Pedido.Application.DTOs.Request;
using Pedido.Application.DTOs.Response;
using Pedido.Application.Events;
using Pedido.Application.Interfaces;
using Pedido.Application.Interfaces.Integrations.PedidoDestino;
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
        private readonly IPedidoDestinoService _pedidoDestinoService;
        private readonly IMediator _mediator;
        private readonly IOptionsMonitor<EnvioPedidosOptions> _optionsMonitor;

        public PedidoService(IPedidoRepository pedidoRepository, IFeatureManager featureManager,
            ILogger<PedidoService> logger, IMapper mapper, IPedidoDestinoService pedidoDestinoService, IMediator mediator, IOptionsMonitor<EnvioPedidosOptions> optionsMonitor)
        {
            _pedidoRepository = pedidoRepository ?? throw new ArgumentNullException(nameof(pedidoRepository));
            _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _pedidoDestinoService = pedidoDestinoService ?? throw new ArgumentNullException(nameof(pedidoDestinoService));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        }

        public async Task<CriarPedidoResponseDTO> CriarPedidoAsync(CriarPedidoRequestDTO requestDto)
        {
            try
            {
                var pedidoDuplicado = await _pedidoRepository.PedidoExisteAsync(requestDto.PedidoId);

                if (pedidoDuplicado)
                    throw new InvalidOperationException("Este pedido já existe.");

                var pedido = _mapper.Map<PedidoEntity>(requestDto);
                pedido.DefinirStatus(PedidoStatus.Criado);

                var usarNovaRegra = await _featureManager.IsEnabledAsync(FeatureFlags.UsarNovaRegraImposto);
                pedido.CalcularImposto(usarNovaRegra);

                await _pedidoRepository.AdicionarAsync(pedido);
                await _pedidoRepository.SalvarAlteracoesAsync();

                return _mapper.Map<CriarPedidoResponseDTO>(pedido);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar o pedido com Id: {Id}", requestDto.PedidoId);
                throw new ApplicationException($"Erro ao criar o pedido. {ex.Message}", ex);
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
                _logger.LogError(ex, "Erro ao consultar o pedido com Id: {Id}", id);
                throw new ApplicationException($"Erro ao consultar o pedido. {ex.Message}", ex);
            }
        }


        public async Task<List<ConsultarPedidoResponseDTO>> ListarPedidosPorStatusAsync(PedidoStatus status)
        {
            try
            {
                var pedidos = await _pedidoRepository.ObterPedidosPorStatusAsync(status);

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
                throw new ApplicationException($"Erro ao cancelar o pedido. {ex.Message}", ex);
            }
        }

        public async Task<int> EnviarPedidosCriadosAsync()
        {
            int totalEnviados = 0;

            var tentativas = _optionsMonitor.CurrentValue.MaxTentativas;
            var pedidos = await _pedidoRepository.ObterPedidosElegiveisParaEnvioAsync(tentativas) ?? new List<PedidoEntity>();

            foreach (var pedido in pedidos)
            {
                try
                {
                    var pedidoDTO = _mapper.Map<ConsultarPedidoResponseDTO>(pedido);
                    await _mediator.Publish(new PedidoCriadoEvent(pedidoDTO));
                    pedido.EnviarPedido();
                    totalEnviados++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao enviar pedido {PedidoId}", pedido.PedidoId);
                    pedido.ErroAoEnviarPedido();
                }
            }

            try
            {
                await _pedidoRepository.SalvarAlteracoesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar alterações após envio dos pedidos.");
                throw new ApplicationException("Erro ao salvar alterações no envio dos pedidos.", ex);
            }

            return totalEnviados;
        }
    }
}
