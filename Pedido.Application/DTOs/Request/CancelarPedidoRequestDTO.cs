using Pedido.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Pedido.Application.DTOs.Request
{
    public class CancelarPedidoRequestDTO
    {
        [Required(ErrorMessage = "Justifique motivo do cancelamento!")]
        [SwaggerSchema("Justificativa para o cancelamento")]
        public string JustificativaCancelamento { get; set; }
    }
}
