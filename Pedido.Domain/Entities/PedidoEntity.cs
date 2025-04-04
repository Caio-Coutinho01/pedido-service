using Pedido.Domain.Enums;

namespace Pedido.Domain.Entities
{
    public class PedidoEntity
    {
        public int Id { get; private set; }
        public int PedidoId { get; private set; }
        public int ClienteId { get; private set; }
        public decimal Imposto { get; private set; }
        public PedidoStatus Status { get; private set; }
        public string? JustificativaCancelamento { get; private set; }
        public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;
        public DateTime? DataEnvio { get; private set; }

        public List<PedidoItemEntity> Itens { get; private set; } = new();

        public static PedidoEntity Criar(int pedidoId, int clienteId, PedidoStatus status, List<PedidoItemEntity> itens, bool calcularImposto = true)
        {
            var pedido = new PedidoEntity
            {
                PedidoId = pedidoId,
                ClienteId = clienteId,
                Status = status,
                Itens = itens
            };
             pedido.CalcularImposto(calcularImposto);

            return pedido;
        }

        public void DefinirStatus(PedidoStatus status)
        {
            Status = status;
        }


        private decimal CalculaValorTotalItens()
        {
            return Itens.Sum(i => i.Valor * i.Quantidade);
        }

        public void CalcularImposto(bool usarNovaRegra)
        {
            var total = CalculaValorTotalItens();
            Imposto = usarNovaRegra ? total * 0.2m : total * 0.3m;
        }

        public void CancelarPedido(string justificativa)
        {
            JustificativaCancelamento = justificativa;
            Status = PedidoStatus.Cancelado;
        }

        public void EnviarPedido()
        {
            if (Status != PedidoStatus.Criado)
                throw new InvalidOperationException("Só é possível processar pedidos com status 'Criado'.");

            DataEnvio = DateTime.UtcNow;
            Status = PedidoStatus.Enviado;
        }
    }
}
