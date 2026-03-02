using AutoMapper;
using EHive.Core.Library.Constants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Entities.Users;
using EHive.Core.Library.Extensions;

namespace EHive.Users.Domain.Mappers
{
    public class MappersConfig : Profile
    {
        public MappersConfig()
        {
            MapUserToUserDto();
            MapTempPasswordToTempPasswordDto();
            MapNewUserDtoToUser();
            MapAddressToAddressDto();
            MapRoleToRoleDto();
            MapUserDocumentToUserDocumentDto();
            MapUserEmailToUserEmailDto();
            MapUserGroupToUserGroupDto();
            MapUserProfileToUserProfileDto();
            MapUserProfileToUserProfileCompleteDto();
        }

        private static string GetLogin(SignInUserDto user)
        {
            var result = user.Login;

            if (string.IsNullOrEmpty(result))
            {
                result = user.Email.Split('@')[0];
                result += !string.IsNullOrEmpty(user.Name) ? $"_{user.Name.Replace(" ", "")}" : string.Empty;
                result += !string.IsNullOrEmpty(user.Surname) ? $"_{user.Surname.Replace(" ", "")}" : string.Empty;
                result += $"_{new Random().Next(1000, 9999)}";
            }
            return result;
        }

        private static string GetLogin(UserDto user)
        {
            var result = user.Login;

            if (string.IsNullOrEmpty(result))
            {
                result = user.MainEmail.Split('@')[0];
                result += !string.IsNullOrEmpty(user.Name) ? $"_{user.Name.Replace(" ", "")}" : string.Empty;
                result += !string.IsNullOrEmpty(user.Surname) ? $"_{user.Surname.Replace(" ", "")}" : string.Empty;
                result += $"_{new Random().Next(1000, 9999)}";
            }
            return result;
        }

        private static List<UserEmail> GetEmails(SignInUserDto user)
        {
            var result = new List<UserEmail>
            {
                new UserEmail { Email = user.Email, IsMain = true, IsValidated = false }
            };
            return result;
        }

        private static List<UserDocument> GetDocuments(SignInUserDto user)
        {
            var result = new List<UserDocument>();
            if (user.Document != null && !string.IsNullOrEmpty(user.Document.DocumentNumber))
            {
                result.Add(new UserDocument { DocumentNumber = user.Document.DocumentNumber, DocumentType = user.Document.DocumentType });
            }
            return result;
        }

        private void MapUserToUserDto()
        {
            CreateMap<User, UserDto>()
                .ReverseMap()
                .ForMember(dest => dest.Login, opt => opt.MapFrom(src => GetLogin(src)));
        }

        private void MapUserDocumentToUserDocumentDto()
        {
            CreateMap<UserDocument, UserDocumentDto>()
                .ReverseMap();
        }

        private void MapTempPasswordToTempPasswordDto()
        {
            CreateMap<TempPassword, TempPasswordDto>()
                .ReverseMap();
        }

        private void MapNewUserDtoToUser()
        {
            CreateMap<SignInUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password.HashMd5()))
                .ForMember(dest => dest.Login, opt => opt.MapFrom(src => GetLogin(src)))
                .ForPath(dest => dest.Documents, opt => opt.MapFrom(src => GetDocuments(src)))
                .ForPath(dest => dest.Emails, opt => opt.MapFrom(src => GetEmails(src)))
                .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => "BR"));
        }

        private void MapAddressToAddressDto()
        {
            CreateMap<Address, AddressDto>()
                .ForPath(dest => dest.State, opt => opt.MapFrom(src => new StateDto { Code = src.State, Name = AddressConstants.GetStateName(src.State) }))
                .ForPath(dest => dest.Country, opt => opt.MapFrom(src => new CountryDto { Code = src.Country, Name = AddressConstants.GetCountryName(src.Country) }))
                .ReverseMap()
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State.Code))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country.Code));
        }

        private void MapRoleToRoleDto()
        {
            CreateMap<Role, RoleDto>()
                .ReverseMap();
        }

        private void MapUserEmailToUserEmailDto()
        {
            CreateMap<UserEmail, UserEmailDto>()
                .ReverseMap();
        }

        private void MapUserGroupToUserGroupDto()
        {
            CreateMap<UserGroup, UserGroupDto>()
                .ReverseMap();
        }

        private void MapUserProfileToUserProfileDto()
        {
            CreateMap<UserProfile, UserProfileDto>()
                .ReverseMap();
        }

        private void MapUserProfileToUserProfileCompleteDto()
        {
            CreateMap<UserProfile, UserProfileCompleteDto>()
                .ReverseMap();
        }
    }
}