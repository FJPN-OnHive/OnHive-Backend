using MongoDB.Driver;
using EHive.Core.Library.Entities.Users;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Users.Domain.Abstractions.Repositories;

namespace EHive.Users.Repositories
{
    public class UserGroupsRepository : MongoRepositoryBase<UserGroup>, IUserGroupsRepository
    {
        public UserGroupsRepository(MongoDBSettings settings) : base(settings, "UserGroups")
        {
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<UserGroup>(Builders<UserGroup>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}