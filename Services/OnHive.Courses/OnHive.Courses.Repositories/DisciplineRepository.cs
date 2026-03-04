using OnHive.Core.Library.Entities.Courses;
using OnHive.Courses.Domain.Abstractions.Repositories;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using MongoDB.Driver;

namespace OnHive.Courses.Repositories
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