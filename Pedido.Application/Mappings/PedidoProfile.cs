using AutoMapper;
using Pedido.Application.DTOs.Request;
using Pedido.Application.DTOs.Response;
using Pedido.Domain.Entities;
using Pedido.Domain.Enums;

namespace Pedido.Application.Mappings
{
    public class PedidoProfile : Profile
    {
        public PedidoProfile()
        {
            #region Mapeamento de consultas (retornos)
            CreateMap<PedidoEntity, ConsultarPedidoResponseDTO>()
                .ForMember(dest => dest.JustificativaCancelamento, opt => opt.Condition(src => src.Status == PedidoStatus.Cancelado));
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
