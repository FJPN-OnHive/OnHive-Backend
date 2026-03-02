using EHive.Core.Library.Entities.Courses;
using EHive.Courses.Domain.Abstractions.Repositories;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using MongoDB.Driver;

namespace EHive.Courses.Repositories
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