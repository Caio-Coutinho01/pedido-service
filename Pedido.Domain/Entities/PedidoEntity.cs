using Pedido.Domain.Enums;

namespace Pedido.Domain.Entities
{
    public class PedidoEntity
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ClienteId { get; set; }
        public decimal Imposto { get; set; }
        public PedidoStatus Status { get; set; }

        public List<PedidoItemEntity> Itens { get; set; } = new();

        public decimal CalculadValorTotalItens()
        {
            return Itens.Sum(i => i.Valor * i.Quantidade);
        }

        public void CalcularImposto(bool usarNovaRegra)
        {
            var total = CalculadValorTotalItens();
            Imposto = usarNovaRegra ? total * 0.2m : total * 0.3m;
        }
    }
}
