using AutoMapper;
using EHive.Core.Library.Contracts.Catalog;
using EHive.Core.Library.Contracts.Search;
using EHive.Core.Library.Entities.Catalog;
using EHive.Core.Library.Entities.Search;

namespace EHive.Search.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapSearchResultToSearchResultDto();
            MapProductCourseSearchResultToProductCourseSearchResultDto();
        }

        private void MapSearchResultToSearchResultDto()
        {
            CreateMap<SearchResult, SearchResultDto>()
                .ReverseMap();
        }

        private void MapProductCourseSearchResultToProductCourseSearchResultDto()
        {
            CreateMap<Installment, InstallmentDto>()
                .ReverseMap();

            CreateMap<ProductPrice, ProductPriceDto>()
                .ReverseMap();

            CreateMap<ProductCourseSearch, ProductCourseSearchDto>()
                .ReverseMap();
        }
    }
}