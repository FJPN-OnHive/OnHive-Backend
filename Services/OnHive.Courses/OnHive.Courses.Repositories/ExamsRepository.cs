using OnHive.Core.Library.Entities.Courses;
using OnHive.Courses.Domain.Abstractions.Repositories;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using MongoDB.Driver;

namespace OnHive.Courses.Repositories
{
    public class ExamsRepository : MongoRepositoryBase<Exam>, IExamsRepository
    {
        public ExamsRepository(MongoDBSettings settings) : base(settings, "Exams")
        {
        }

        public async Task<List<Exam>> GetAllIdMigrate()
        {
            var filter = Builders<Exam>.Filter.ElemMatch(i => i.Questions, Builders<ExamQuestion>.Filter.Eq(q => q.Id, null));
            return await collection.Find(filter).ToListAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Exam>(Builders<Exam>.IndexKeys.Ascending(i => i.TenantId)));
        }
    }
}