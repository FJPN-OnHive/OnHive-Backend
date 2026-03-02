using EHive.Core.Library.Entities.Courses;
using EHive.Courses.Domain.Abstractions.Repositories;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using MongoDB.Driver;

namespace EHive.Courses.Repositories
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