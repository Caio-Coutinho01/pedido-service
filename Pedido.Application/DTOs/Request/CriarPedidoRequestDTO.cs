using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Pedido.Application.DTOs.Request
{
    public class CriarPedidoRequestDTO
    {
        [SwaggerSchema("Identificador único do pedido")]
        [Required]
        public int PedidoId { get; set; }

        [SwaggerSchema("Identificador do cliente")]
        [Required]
        public int ClienteId { get; set; }

        [SwaggerSchema("Lista de itens do pedido")]
        [MinLength(1, ErrorMessage = "O pedido deve conter ao menos um item.")]
        public List<ItemPedidoDTO> Itens { get; set; }
    }

    public class ItemPedidoDTO
    {
        [SwaggerSchema("Identificador do produto")]
        [Required]
        public int ProdutoId { get; set; }

        [SwaggerSchema("Quantidade de itens")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
        public int Quantidade { get; set; }

        [SwaggerSchema("Valor unitário do item")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }
    }
}
