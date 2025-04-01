using Microsoft.AspNetCore.Mvc;
using Pedido.Application.DTOs;
using Pedido.Application.Interfaces;
using Pedido.Domain.Enums;

namespace Pedido.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;

        public PedidoController(IPedidoService pedidoService)
        {
            _pedidoService = pedidoService ?? throw new ArgumentNullException(nameof(pedidoService));
        }


        [HttpPost]
        public async Task<IActionResult> CriarPedido(CriarPedidoRequestDTO request)
        {
            try
            {
                var response = await _pedidoService.CriarPedidoAsync(request);
                return CreatedAtAction(nameof(ConsultarPedido), new { id = response.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ConsultarPedido(int id)
        {
            var pedido = await _pedidoService.ConsultarPedidoPorIdAsync(id);

            if (pedido == null)
                return NotFound(new {erro = "Pedido não encontrado." });

            return Ok(pedido);
        }

        [HttpGet("status")]
        public async Task<IActionResult> ListarPorStatus([FromQuery] PedidoStatus status)
        {
            try
            {
                var pedidos = await _pedidoService.ListarPedidosPorStatusAsync(status);
                return Ok(pedidos);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }

    }
}
