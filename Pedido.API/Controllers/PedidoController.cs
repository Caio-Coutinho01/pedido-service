using Microsoft.AspNetCore.Mvc;
using Pedido.Application.DTOs.Request;
using Pedido.Application.Interfaces;
using Pedido.Domain.Enums;

namespace Pedido.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;
        private readonly IHostEnvironment _env;

        public PedidoController(IPedidoService pedidoService, IHostEnvironment env)
        {
            _pedidoService = pedidoService ?? throw new ArgumentNullException(nameof(pedidoService));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        /// <summary>
        /// Criar um novo pedido.
        /// </summary>
        /// <returns>Status de sucesso</returns>
        [HttpPost]
        public async Task<IActionResult> CriarPedido(CriarPedidoRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _pedidoService.CriarPedidoAsync(request);
                return CreatedAtAction(nameof(ConsultarPedido), new { id = response.Id }, response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { erro = ex.InnerException?.Message ?? ex.Message });
            }
        }

        /// <summary>
        /// Consultar um pedido existente pelo Id.
        /// </summary>
        /// <param name="id">Identificador do pedido</param>
        /// <returns>Status de sucesso</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> ConsultarPedido(int id)
        {
            var pedido = await _pedidoService.ConsultarPedidoPorIdAsync(id);

            if (pedido == null)
                return NotFound(new {erro = "Pedido não encontrado." });

            return Ok(pedido);
        }

        /// <summary>
        /// Consultar pedidos por status.
        /// </summary>
        /// <param name="status"></param>
        /// <returns>Status de sucesso</returns>
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
                return BadRequest(new { erro = ex.InnerException?.Message ?? ex.Message });
            }
        }

        /// <summary>
        /// Cancelar um pedido existente.
        /// </summary>
        /// <param name="id">Identificador do pedido</param>
        /// <param name="dto">Justificativa do cancelamento</param>
        /// <returns>Status de sucesso</returns>
        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> CancelarPedido(int id, [FromBody] CancelarPedidoRequestDTO dto)
        {
            try
            {
                var resultado = await _pedidoService.CancelarPedidoAsync(id, dto);
                return Ok(resultado);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }

        /// <summary>
        /// Enviar todos os pedidos já criados.
        /// </summary>
        /// <returns></returns>
        [HttpPost("enviar-pedidos-elegiveis")]
        public async Task<IActionResult> EnviarPedidosElegiveis()
        {
            try
            {
                int totalEnviados = await _pedidoService.EnviarPedidosCriadosAsync();
                return Ok(new { TotalEnviados = totalEnviados });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { erro = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost("seed")]
        public async Task<IActionResult> CriarPedidosFake([FromQuery] int quantidade = 50)
        {
            if (!_env.IsDevelopment())
                return Forbid("Esse endpoint só pode ser usado em ambiente de desenvolvimento.");

            for (int i = 0; i < quantidade; i++)
            {
                var request = new CriarPedidoRequestDTO
                {
                        PedidoId = 900000 + i,
                        ClienteId = 123,
                        Itens = new List<ItemPedidoDTO>{new ItemPedidoDTO {ProdutoId = 5000 + i, Quantidade = 1,Valor = 10 + i}}
                };

                await _pedidoService.CriarPedidoAsync(request);
            }

            return Ok($"{quantidade} pedidos criados com sucesso.");
        }
    }
}
