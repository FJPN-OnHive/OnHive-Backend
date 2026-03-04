using AutoMapper;
using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Search;
using OnHive.Core.Library.Entities.Catalog;
using OnHive.Core.Library.Entities.Search;

namespace OnHive.Search.Domain.Mappers
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