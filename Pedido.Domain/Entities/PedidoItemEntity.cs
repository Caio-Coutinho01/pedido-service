namespace Pedido.Domain.Entities
{
    public class PedidoItemEntity
    {
        public int Id { get; private set; }
        public int ProdutoId { get; private set; }
        public int Quantidade { get; private set; }
        public decimal Valor { get; private set; }

        public int PedidoEntityId { get; private set; }
        public PedidoEntity Pedido { get; private set; }

        public PedidoItemEntity(int produtoId, int quantidade, decimal valor)
        {
            ProdutoId = produtoId;
            Quantidade = quantidade;
            Valor = valor;
        }

        private PedidoItemEntity() { }

    }
}
