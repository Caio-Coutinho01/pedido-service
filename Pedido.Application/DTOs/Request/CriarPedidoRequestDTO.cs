using Swashbuckle.AspNetCore.Annotations;

namespace Pedido.Application.DTOs.Request
{
    public class CriarPedidoRequestDTO
    {
        [SwaggerSchema("Identificador único do pedido")]
        public int PedidoId { get; set; }

        [SwaggerSchema("Identificador do cliente")]
        public int ClienteId { get; set; }

        [SwaggerSchema("Lista de itens do pedido")]
        public List<ItemPedidoDTO> Itens { get; set; }
    }

    public class ItemPedidoDTO
    {
        [SwaggerSchema("Identificador do produto")]
        public int ProdutoId { get; set; }

        [SwaggerSchema("Quantidade de itens")]
        public int Quantidade { get; set; }

        [SwaggerSchema("Valor unitário do item")]
        public decimal Valor { get; set; }
    }
}
