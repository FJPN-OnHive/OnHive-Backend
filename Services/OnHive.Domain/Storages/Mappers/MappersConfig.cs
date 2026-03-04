using AutoMapper;
using OnHive.Core.Library.Contracts.Storages;
using OnHive.Core.Library.Entities.Storages;

namespace OnHive.Storages.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapStorageImageToStorageImageDto();
            MapStorageFileToStorageFileDto();
        }

        private void MapStorageImageToStorageImageDto()
        {
            CreateMap<StorageImageFile, StorageImageFileDto>()
                .ReverseMap();
        }

        private void MapStorageFileToStorageFileDto()
        {
            CreateMap<StorageFile, StorageFileDto>()
                .ReverseMap();
        }
    }
}