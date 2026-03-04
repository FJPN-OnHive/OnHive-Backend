using MongoDB.Driver;
using OnHive.Core.Library.Entities.Teachers;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Teachers.Domain.Abstractions.Repositories;

namespace OnHive.Teachers.Repositories
{
    public class TeachersRepository : MongoRepositoryBase<Teacher>, ITeachersRepository
    {
        public TeachersRepository(MongoDBSettings settings) : base(settings, "Teachers")
        {
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Teacher>(Builders<Teacher>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}
