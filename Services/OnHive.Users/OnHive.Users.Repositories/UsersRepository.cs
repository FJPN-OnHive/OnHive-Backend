using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Entities.Users;
using OnHive.Core.Library.Enums.Users;
using OnHive.Core.Library.Helpers;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Users.Domain.Abstractions.Repositories;
using OnHive.Users.Domain.Models;
using MongoDB.Driver;

namespace OnHive.Users.Repositories
{
    public class UsersRepository : MongoRepositoryBase<User>, IUsersRepository
    {
        public UsersRepository(MongoDBSettings settings) : base(settings, "Users")
        {
        }

        public override async Task<PaginatedResult<User>> GetByFilterAsync(RequestFilter filter, string? tenantId, bool activeOnly = true)
        {
            var queryFilter = MongoDbFilterConverter.ConvertFilter<UserSearch>(filter, tenantId, activeOnly);

            var result = collection
               .Aggregate()
               .Project(u => new UserSearch
               {
                   Id = u.Id,
                   TenantId = u.TenantId,
                   Login = u.Login,
                   Name = u.Name,
                   Emails = u.Emails,
                   Email = u.Emails.FirstOrDefault(e => e.IsMain) == null ? u.Emails.FirstOrDefault().Email : u.Emails.FirstOrDefault(e => e.IsMain).Email,
                   Documents = u.Documents,
                   Document = u.Documents.FirstOrDefault() == null ? string.Empty : u.Documents.FirstOrDefault().DocumentNumber,
                   IsActive = u.IsActive,
                   Addresses = u.Addresses,
                   BirthDate = u.BirthDate,
                   ActiveVersion = u.ActiveVersion,
                   ChangePasswordCodes = u.ChangePasswordCodes,
                   Gender = u.Gender,
                   IsChangePasswordRequested = u.IsChangePasswordRequested,
                   IsForeigner = u.IsForeigner,
                   MobilePhoneNumber = u.MobilePhoneNumber,
                   Nationality = u.Nationality,
                   PasswordHash = u.PasswordHash,
                   Occupation = u.Occupation,
                   PhoneNumber = u.PhoneNumber,
                   RedirectLoginCode = u.RedirectLoginCode,
                   SocialLoginToken = u.SocialLoginToken,
                   SocialName = u.SocialName,
                   Version = u.Version,
                   Roles = u.Roles,
                   Surname = u.Surname,
                   VersionNumber = u.VersionNumber,
                   VId = u.VId,
                   CreatedBy = u.CreatedBy,
                   UpdatedBy = u.UpdatedBy,
                   CreatedAt = u.CreatedAt,
                   UpdatedAt = u.UpdatedAt
               })
               .Match(queryFilter)
               .Project(u => (User)u);

            var count = (await collection
                .Aggregate()
                .Project(u => new UserSearch
                {
                    Id = u.Id,
                    TenantId = u.TenantId,
                    Login = u.Login,
                    Name = u.Name,
                    Emails = u.Emails,
                    Email = u.Emails.FirstOrDefault(e => e.IsMain) == null ? u.Emails.FirstOrDefault().Email : u.Emails.FirstOrDefault(e => e.IsMain).Email,
                    Documents = u.Documents,
                    Document = u.Documents.FirstOrDefault() == null ? string.Empty : u.Documents.FirstOrDefault().DocumentNumber,
                    IsActive = u.IsActive,
                    Addresses = u.Addresses,
                    BirthDate = u.BirthDate,
                    ActiveVersion = u.ActiveVersion,
                    ChangePasswordCodes = u.ChangePasswordCodes,
                    Gender = u.Gender,
                    IsChangePasswordRequested = u.IsChangePasswordRequested,
                    IsForeigner = u.IsForeigner,
                    MobilePhoneNumber = u.MobilePhoneNumber,
                    Nationality = u.Nationality,
                    PasswordHash = u.PasswordHash,
                    Occupation = u.Occupation,
                    PhoneNumber = u.PhoneNumber,
                    RedirectLoginCode = u.RedirectLoginCode,
                    SocialLoginToken = u.SocialLoginToken,
                    SocialName = u.SocialName,
                    Version = u.Version,
                    Roles = u.Roles,
                    Surname = u.Surname,
                    VersionNumber = u.VersionNumber,
                    VId = u.VId,
                    CreatedBy = u.CreatedBy,
                    UpdatedBy = u.UpdatedBy,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .Match(queryFilter)
                .Count()
                .FirstOrDefaultAsync())?
                .Count ?? 0;

            var pageCount = 1L;
            if (filter.Sort.Any())
            {
                result.Sort(MongoDbFilterConverter.ConvertSort<User>(filter));
            }
            else
            {
                result.Sort(Builders<User>.Sort.Descending("UpdatedAt"));
            }

            if (filter.PageLimit > 0)
            {
                if (filter.Page <= 0) filter.Page = 1;
                result = result
                            .Skip((filter.Page - 1) * filter.PageLimit)
                            .Limit(filter.PageLimit);
                pageCount = (long)Math.Ceiling((double)count / filter.PageLimit);
            }

            return new PaginatedResult<User>
            {
                Page = filter.Page,
                PageCount = pageCount,
                Total = count,
                Itens = await result.ToListAsync()
            };
        }

        public async Task<PaginatedResult<User?>> GetByFilterAndProfileAsync(RequestFilter filter, ProfileTypes profile, string? tenantId, bool activeOnly = true)
        {
            var queryFilter = MongoDbFilterConverter.ConvertFilter<User>(filter, tenantId, activeOnly);

            var result = collection
                .Aggregate()
                .Match(queryFilter)
                .Lookup<User, UserWithProfile>("UserProfiles", "Id", "UserId", "Profiles")
                .Match(Builders<UserWithProfile>.Filter.ElemMatch(u => u.Profiles, p => p.Type == profile))
                .Project(u => (User)u);

            var count = (await collection
                .Aggregate()
                .Match(queryFilter)
                .Lookup<User, UserWithProfile>("UserProfiles", "Id", "UserId", "Profiles")
                .Match(Builders<UserWithProfile>.Filter.ElemMatch(u => u.Profiles, p => p.Type == profile))
                .Count()
                .FirstOrDefaultAsync())?
                .Count ?? 0;

            var pageCount = 1L;
            if (filter.Sort.Any())
            {
                result.Sort(MongoDbFilterConverter.ConvertSort<User>(filter));
            }
            else
            {
                result.Sort(Builders<User>.Sort.Descending("UpdatedAt"));
            }

            if (filter.PageLimit > 0)
            {
                if (filter.Page <= 0) filter.Page = 1;
                result = result
                            .Skip((filter.Page - 1) * filter.PageLimit)
                            .Limit(filter.PageLimit);
                pageCount = (long)Math.Ceiling((double)count / filter.PageLimit);
            }

            return new PaginatedResult<User>
            {
                Page = filter.Page,
                PageCount = pageCount,
                Total = count,
                Itens = await result.ToListAsync()
            };
        }

        public async Task<User?> GetByMainEmailAsync(string email, string tenantId)
        {
            var filter = Builders<User>.Filter.ElemMatch(u => u.Emails, e => e.Email.ToLower() == email.ToLower())
                & Builders<User>.Filter.Eq(u => u.TenantId, tenantId);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByEmailAsync(string email, string tenantId)
        {
            var filter = Builders<User>.Filter.ElemMatch(u => u.Emails, e => e.Email.ToLower() == email.ToLower())
                & Builders<User>.Filter.Eq(u => u.TenantId, tenantId);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByLoginAsync(string login, string tenantId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Login, login)
               & Builders<User>.Filter.Eq(u => u.TenantId, tenantId);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByLoginCodeAsync(string code)
        {
            var filter = Builders<User>.Filter.Eq(u => u.RedirectLoginCode.Code, code);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByMainEmailCodeAsync(string code, string tenantId)
        {
            var filter = Builders<User>.Filter.ElemMatch(u => u.Emails, e => e.ValidationCodes.Any(c => c.Code == code) && e.IsMain)
               & Builders<User>.Filter.Eq(u => u.TenantId, tenantId);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByRecoverPasswordCodeAsync(string code, string tenantId)
        {
            var filter = Builders<User>.Filter.ElemMatch(u => u.ChangePasswordCodes, e => e.Code == code)
               & Builders<User>.Filter.Eq(u => u.TenantId, tenantId);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(i => i.Login)));
            collection.Indexes.CreateOne(new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}