using AutoMapper;
using Pedido.Application.DTOs;
using Pedido.Domain.Entities;

namespace Pedido.Application.Mappings
{
    public class PedidoProfile : Profile
    {
        public PedidoProfile()
        {
            #region Mapeamento de consultas (retornos)
            CreateMap<PedidoEntity, ConsultarPedidoResponseDTO>();
            CreateMap<PedidoItemEntity, ItemPedidoDTO>();
            CreateMap<PedidoEntity, CriarPedidoResponseDTO>();
            #endregion

            #region Mapeamento de criação (entradas)
            CreateMap<CriarPedidoRequestDTO, PedidoEntity>();
            CreateMap<ItemPedidoDTO, PedidoItemEntity>();
            #endregion
        }
    }
}
