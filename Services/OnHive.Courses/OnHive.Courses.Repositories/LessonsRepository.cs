using OnHive.Core.Library.Entities.Courses;
using OnHive.Courses.Domain.Abstractions.Repositories;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using MongoDB.Driver;

namespace OnHive.Courses.Repositories
{
    public class LessonsRepository : MongoRepositoryBase<Lesson>, ILessonsRepository
    {
        public LessonsRepository(MongoDBSettings settings) : base(settings, "Lessons")
        {
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Lesson>(Builders<Lesson>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}