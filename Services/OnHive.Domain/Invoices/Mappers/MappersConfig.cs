using AutoMapper;
using OnHive.Core.Library.Contracts.Invoices;
using OnHive.Core.Library.Entities.Invoices;

namespace OnHive.Invoices.Domain.Mappers
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