using AutoMapper;
using EHive.Core.Library.Contracts.Invoices;
using EHive.Core.Library.Entities.Invoices;

namespace EHive.Invoices.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapInvoiceToInvoiceDto();
        }

        private void MapInvoiceToInvoiceDto()
        {
            CreateMap<InvoiceItens, InvoiceItensDto>()
                    .ReverseMap();

            CreateMap<InvoiceEmitter, InvoiceEmitterDto>()
                .ReverseMap();

            CreateMap<InvoiceClient, InvoiceClientDto>()
                .ReverseMap();

            CreateMap<Invoice, InvoiceDto>()
                .ReverseMap();
        }
    }
}