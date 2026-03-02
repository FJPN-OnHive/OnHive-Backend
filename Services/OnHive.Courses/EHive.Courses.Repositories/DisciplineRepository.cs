using EHive.Core.Library.Entities.Courses;
using EHive.Courses.Domain.Abstractions.Repositories;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using MongoDB.Driver;

namespace EHive.Courses.Repositories
{
    public class DisciplineRepository : MongoRepositoryBase<Discipline>, IDisciplineRepository
    {
        public DisciplineRepository(MongoDBSettings settings) : base(settings, "Disciplines")
        {
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Discipline>(Builders<Discipline>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}