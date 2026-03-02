using AutoMapper;
using EHive.Core.Library.Contracts.Storages;
using EHive.Core.Library.Entities.Storages;

namespace EHive.Storages.Domain.Mappers
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