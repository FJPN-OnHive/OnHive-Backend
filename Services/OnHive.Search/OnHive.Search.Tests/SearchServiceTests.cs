using AutoMapper;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using OnHive.Core.Library.Contracts.Search;
using OnHive.Core.Library.Contracts.Tenants;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Entities.Search;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Search.Domain.Abstractions.Repositories;
using OnHive.Search.Domain.Mappers;
using OnHive.Search.Domain.Models;
using OnHive.Search.Services;
using System.Text.Json;
using OnHive.Core.Library.Contracts.Courses;
using System.Net;

namespace OnHive.Search.Tests
{
    public class SearchServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<ISearchRepository> mockSearchRepository;
        private readonly SearchApiSettings searchApiSettings;
        private readonly IMapper mapper;

        public SearchServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockSearchRepository = mockRepository.Create<ISearchRepository>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            searchApiSettings = new SearchApiSettings();
            searchApiSettings.SearchAdminPermission = "search_admin";
            searchApiSettings.Targets = new List<Target>
            {
                new Target
                {
                    Type = "Test",
                    CollectionName = "TEST",
                    ValueField = "Value",
                    DescriptionField = "Description",
                    ImageField = "ImageUrl",
                    ImageAltTextField = "",
                    SlugField = "Slug",
                    ActiveCriteria = new List<ActiveCriteria>
                    {
                        new ActiveCriteria
                        {
                            Field = "IsActive",
                            Values = "true"
                        }
                    }
                }
            };
        }

        [Fact]
        public async Task GetSearchResultByFilter_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var expected = new List<SearchResult>
            {
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = loggedUser!.User!.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = loggedUser!.User!.TenantId
                },
                new()
                {
                   Id = Guid.NewGuid().ToString(),
                   TenantId = loggedUser!.User!.TenantId
                }
            };

            var filter = new RequestFilter
            {
                PageLimit = 10,
                Page = 1,
                Filter = new List<FilterField>
                {
                    new FilterField
                    {
                        Field = "TenantId",
                        Operator = "==",
                        Value = loggedUser!.User!.TenantId
                    }
                }
            };

            var coursesResult = new PaginatedResult<CourseDto>
            {
                Page = 1,
                PageCount = 1,
                Itens = new List<CourseDto>
                {
                    new CourseDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        TenantId = loggedUser!.User!.TenantId,
                        ImageUrl = "http://test.com/image1.jpg",
                        Slug  = "test"
                    }
                }
            };

            var resultResponse = Response<PaginatedResult<CourseDto>>.Ok(coursesResult);

            mockSearchRepository.Setup(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, true))
                .ReturnsAsync(new PaginatedResult<SearchResult> { Page = 1, PageCount = 1, Itens = expected });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<List<SearchResultDto>>();
            result?.Should().NotBeNull();
            result?.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetSearchResultsByFilter_NotFound_Test()
        {
            // Arrange
            var service = CreateService();

            var loggedUser = GetTestUser();

            var filter = new RequestFilter
            {
                PageLimit = 10,
                Page = 1,
                Filter = new List<FilterField>
                {
                    new FilterField
                    {
                        Field = "TenantId",
                        Operator = "==",
                        Value = loggedUser!.User!.TenantId
                    }
                }
            };

            var coursesResult = new PaginatedResult<CourseDto>
            {
                Page = 1,
                PageCount = 1,
                Itens = []
            };

            mockSearchRepository.Setup(r => r.GetByFilterAsync(filter, loggedUser!.User!.TenantId, true))
             .ReturnsAsync(new PaginatedResult<SearchResult> { Page = 1, PageCount = 1, Itens = [] });

            // Act
            var result = await service.GetByFilterAsync(filter, loggedUser);

            // Assert
            result?.Should().NotBeNull();
            result?.Should().BeOfType<List<SearchResultDto>>();
            result?.Should().NotBeNull();
            result?.Should().HaveCount(0);
        }

        private SearchService CreateService()
        {
            return new SearchService(
                mockSearchRepository.Object,
                searchApiSettings,
                mapper);
        }

        private LoggedUserDto GetTestUser()
        {
            var tenantId = Guid.NewGuid().ToString();
            return new LoggedUserDto(
                new UserDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Test",
                    Login = "Test",
                    Emails = new List<UserEmailDto> { new UserEmailDto { Email = "Test@Test.com", IsMain = true, IsValidated = true } },
                    IsActive = true,
                    Permissions = new List<string> { "admin", "search_admin" },
                    Roles = { "admin", "staff" },
                    TenantId = tenantId,
                    Tenant = new TenantDto
                    {
                        Id = tenantId,
                        Domain = "TestCo",
                        Email = "Test@TestCo.com",
                        Name = "TestCo",
                        Features = new List<string> { "homolog" }
                    },
                },
                "TOKEN"
            );
        }
    }
}