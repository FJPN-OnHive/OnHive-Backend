using MongoDB.Driver;
using OnHive.Core.Library.Entities.Users;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Users.Domain.Abstractions.Repositories;

namespace OnHive.Users.Repositories
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