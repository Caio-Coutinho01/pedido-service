using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using NSubstitute;
using Pedido.Application.Configuration;
using Pedido.Application.DTOs.Request;
using Pedido.Application.Interfaces.Integrations.PedidoDestino;
using Pedido.Application.Mappings;
using Pedido.Application.Services;
using Pedido.Domain.Enums;
using Pedido.Infrastructure.Repositories;
using Pedido.Tests.Builders.Base;

namespace Pedido.Tests.Integration.Services
{
    public class PedidoServiceIntegrationTests : IntegrationTestBase
    {
        private PedidoService CriarService()
        {
            var loggerRepo = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<PedidoRepository>();
            var loggerService = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<PedidoService>();

            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<PedidoProfile>());
            var mapper = mapperConfig.CreateMapper();

            var repository = new PedidoRepository(DbContext, loggerRepo);
            var featureManager = Substitute.For<IFeatureManager>();
            featureManager.IsEnabledAsync("UsarNovaRegraImposto").Returns(false);

            var pedidoDestinoService = Substitute.For<IPedidoDestinoService>();
            var mediator = Substitute.For<IMediator>();

            var pedidoOptions = Substitute.For<IOptionsMonitor<EnvioPedidosOptions>>();
            pedidoOptions.CurrentValue.Returns(new EnvioPedidosOptions { MaxTentativas = 3 });

            return new PedidoService(repository, featureManager, loggerService, mapper, pedidoDestinoService, mediator, pedidoOptions);
        }

        [Fact]
        public async Task CriarPedidoAsync_DevePersistirPedidoComSucesso()
        {

            var service = CriarService();

            var request = new CriarPedidoRequestDTO
            {
                PedidoId = 999,
                ClienteId = 456,
                Itens = new List<ItemPedidoDTO>
                {
                    new() { ProdutoId = 1, Quantidade = 2, Valor = 10 }
                }
            };

            var response = await service.CriarPedidoAsync(request);

            response.Should().NotBeNull();
            response.Status.Should().Be("Criado");

            var pedidoDb = DbContext.Pedidos.FirstOrDefault(p => p.PedidoId == 999);
            pedidoDb.Should().NotBeNull();
            pedidoDb!.ClienteId.Should().Be(456);
            pedidoDb.Itens.Should().HaveCount(1);
            pedidoDb.Imposto.Should().BeApproximately(6.0m, 0.01m);
        }

        [Fact]
        public async Task CancelarPedidoAsync_DeveCancelarPedidoComSucesso()
        {
            var service = CriarService();
            var pedido = new CriarPedidoRequestDTO
            {
                PedidoId = 1000,
                ClienteId = 123,
                Itens = new List<ItemPedidoDTO>
                {
                    new() { ProdutoId = 1, Quantidade = 1, Valor = 10 }
                }
            };

            var pedidoResponse = await service.CriarPedidoAsync(pedido);

            var cancelRequest = new CancelarPedidoRequestDTO { JustificativaCancelamento = "Teste de cancelamento" };

            var response = await service.CancelarPedidoAsync(pedidoResponse.Id, cancelRequest);

            response.Should().NotBeNull();
            response.Sucesso.Should().BeTrue();
            response.Mensagem.Should().Contain("cancelado");
            var pedidoDb = await service.ConsultarPedidoPorIdAsync(pedidoResponse.Id);
            pedidoDb?.Status.Should().NotBe(PedidoStatus.Criado.ToString());
        }

        [Fact]
        public async Task ConsultarPedidoPorIdAsync_DeveRetornarPedido_QuandoExistir()
        {
            var service = CriarService();

            var request = new CriarPedidoRequestDTO
            {
                PedidoId = 2000,
                ClienteId = 321,
                Itens = new List<ItemPedidoDTO>
                {
                    new() { ProdutoId = 1, Quantidade = 1, Valor = 10 }
                }
            };

            var createdResponse = await service.CriarPedidoAsync(request);

            var pedidoConsultado = await service.ConsultarPedidoPorIdAsync(createdResponse.Id);

            pedidoConsultado.Should().NotBeNull();
            pedidoConsultado!.PedidoId.Should().Be(request.PedidoId);
        }

        [Fact]
        public async Task ConsultarPedidoPorIdAsync_DeveLancarApplicationException_QuandoPedidoNaoExistir()
        {
            var service = CriarService();

            Func<Task> act = async () => await service.ConsultarPedidoPorIdAsync(999999);

            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage("*Pedido não encontrado com o ID: 999999*");
        }

        [Fact]
        public async Task ListarPedidosPorStatusAsync_DeveRetornarPedidosCorretamente()
        {
            var service = CriarService();

            var request1 = new CriarPedidoRequestDTO
            {
                PedidoId = 3000,
                ClienteId = 111,
                Itens = new List<ItemPedidoDTO>
                {
                    new() { ProdutoId = 1, Quantidade = 1, Valor = 10 }
                }
            };

            var request2 = new CriarPedidoRequestDTO
            {
                PedidoId = 3001,
                ClienteId = 222,
                Itens = new List<ItemPedidoDTO>
                {
                    new() { ProdutoId = 2, Quantidade = 2, Valor = 20 }
                }
            };

            await service.CriarPedidoAsync(request1);
            await service.CriarPedidoAsync(request2);

            await service.EnviarPedidosCriadosAsync();

            var pedidosCriados = await service.ListarPedidosPorStatusAsync(PedidoStatus.Criado);
            var pedidosEnviados = await service.ListarPedidosPorStatusAsync(PedidoStatus.Enviado);

            pedidosCriados.Should().BeEmpty();
            pedidosEnviados.Should().HaveCountGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task EnviarPedidosCriadosAsync_DeveEnviarPedidosEAtualizarStatus()
        {
            var service = CriarService();

            var request1 = new CriarPedidoRequestDTO
            {
                PedidoId = 4000,
                ClienteId = 333,
                Itens = new List<ItemPedidoDTO>
                {
                    new() { ProdutoId = 1, Quantidade = 1, Valor = 10 }
                }
            };

            var request2 = new CriarPedidoRequestDTO
            {
                PedidoId = 4001,
                ClienteId = 444,
                Itens = new List<ItemPedidoDTO>
                {
                    new() { ProdutoId = 2, Quantidade = 2, Valor = 20 }
                }
            };

            await service.CriarPedidoAsync(request1);
            await service.CriarPedidoAsync(request2);

            int totalEnviados = await service.EnviarPedidosCriadosAsync();

            totalEnviados.Should().Be(2);

            var pedidosEnviados = await service.ListarPedidosPorStatusAsync(PedidoStatus.Enviado);
            pedidosEnviados.Should().HaveCountGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task CancelarPedidoAsync_DeveLancarApplicationException_QuandoStatusNaoForCriado()
        {
            var service = CriarService();

            var request = new CriarPedidoRequestDTO
            {
                PedidoId = 5000,
                ClienteId = 555,
                Itens = new List<ItemPedidoDTO>
                {
                    new() { ProdutoId = 1, Quantidade = 1, Valor = 10 }
                }
            };

            var createdResponse = await service.CriarPedidoAsync(request);

            await service.EnviarPedidosCriadosAsync();

            var cancelRequest = new CancelarPedidoRequestDTO { JustificativaCancelamento = "Teste de cancelamento inválido" };

            Func<Task> act = async () => await service.CancelarPedidoAsync(createdResponse.Id, cancelRequest);

            await act.Should().ThrowAsync<ApplicationException>()
                .WithMessage($"Erro ao cancelar o pedido. O Pedido {createdResponse.Id} não pode ser cancelado, pois já está em processamento ou cancelado!*");
        }

    }
}
