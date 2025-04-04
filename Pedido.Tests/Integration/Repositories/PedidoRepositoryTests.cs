using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pedido.Domain.Entities;
using Pedido.Domain.Enums;
using Pedido.Infrastructure.Repositories;
using Pedido.Tests.Builders.Base;

namespace Pedido.Tests.Integration.Repositories
{
    public class PedidoRepositoryTests : IntegrationTestBase
    {
        private PedidoRepository CriarRepository()
        {
            var logger = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            }).CreateLogger<PedidoRepository>();

            return new PedidoRepository(DbContext, logger);
        }

        [Fact]
        public async Task AdicionarPedido_DevePersistirNoBanco()
        {
            var repository = CriarRepository();

            var pedido = PedidoEntity.Criar(999, 456, PedidoStatus.Criado, new List<PedidoItemEntity> { new PedidoItemEntity(1, 2, 10) });

            await repository.AdicionarAsync(pedido);
            await repository.SalvarAlteracoesAsync();

            var pedidoDb = await DbContext.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.PedidoId == 999);

            pedidoDb.Should().NotBeNull();
            pedidoDb!.ClienteId.Should().Be(456);
            pedidoDb.Itens.Should().HaveCount(1);
        }

        [Fact]
        public async Task PedidoExisteAsync_DeveRetornarTrue_QuandoPedidoExiste()
        {
            var repository = CriarRepository();
            var pedido = PedidoEntity.Criar(1234, 321, PedidoStatus.Criado,
                new List<PedidoItemEntity> { new PedidoItemEntity(1, 1, 10) });
            await repository.AdicionarAsync(pedido);
            await repository.SalvarAlteracoesAsync();

            var exists = await repository.PedidoExisteAsync(1234);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task PedidoExisteAsync_DeveRetornarFalse_QuandoPedidoNaoExiste()
        {
            var repository = CriarRepository();

            var exists = await repository.PedidoExisteAsync(99999);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarPedidoQuandoExistir()
        {
            var repository = CriarRepository();
            var pedido = PedidoEntity.Criar(555, 789, PedidoStatus.Criado, new List<PedidoItemEntity> { new(1, 2, 10) });

            await repository.AdicionarAsync(pedido);
            await repository.SalvarAlteracoesAsync();

            var pedidoDb = await repository.ObterPorIdAsync(pedido.Id);

            pedidoDb.Should().NotBeNull();
            pedidoDb!.PedidoId.Should().Be(555);
        }

        [Fact]
        public async Task ObterPedidosPorStatusAsync_DeveRetornarApenasPedidosComStatusInformado()
        {
            var repository = CriarRepository();

            var pedido1 = PedidoEntity.Criar(1, 1, PedidoStatus.Criado, new List<PedidoItemEntity> { new(1, 1, 10) });
            var pedido2 = PedidoEntity.Criar(2, 2, PedidoStatus.Enviado, new List<PedidoItemEntity> { new(2, 1, 20) });

            await repository.AdicionarAsync(pedido1);
            await repository.AdicionarAsync(pedido2);
            await repository.SalvarAlteracoesAsync();

            var pedidosCriados = await repository.ObterPedidosPorStatusAsync(PedidoStatus.Criado);

            pedidosCriados.Should().HaveCount(1);
            pedidosCriados.First().PedidoId.Should().Be(1);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarNull_QuandoPedidoNaoExiste()
        {
            var repository = CriarRepository();

            var pedidoDb = await repository.ObterPorIdAsync(999999);

            pedidoDb.Should().BeNull();
        }

        [Fact]
        public async Task ObterPedidosPorStatusAsync_DeveRetornarListaVazia_QuandoNenhumPedidoTemStatus()
        {
            var repository = CriarRepository();

            var pedido = PedidoEntity.Criar(10, 10, PedidoStatus.Enviado,
                new List<PedidoItemEntity> { new PedidoItemEntity(1, 1, 10) });
            await repository.AdicionarAsync(pedido);
            await repository.SalvarAlteracoesAsync();

            var pedidos = await repository.ObterPedidosPorStatusAsync(PedidoStatus.Criado);

            pedidos.Should().BeEmpty();
        }
    }
}
