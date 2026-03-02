using MongoDB.Driver;
using EHive.Core.Library.Entities.Teachers;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Teachers.Domain.Abstractions.Repositories;

namespace EHive.Teachers.Repositories
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
