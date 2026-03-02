using EHive.Core.Library.Entities.Students;
using EHive.Database.Library.Models;
using EHive.Database.Library.MongoDB;
using EHive.Students.Domain.Abstractions.Repositories;
using MongoDB.Driver;

namespace EHive.Students.Repositories
{
    public class StudentReportsRepository : MongoRepositoryBase<StudentReport>, IStudentReportsRepository
    {
        public StudentReportsRepository(MongoDBSettings settings) : base(settings, "StudentReports")
        {
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<StudentReport>(Builders<StudentReport>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<StudentReport>(Builders<StudentReport>.IndexKeys.Ascending(i => i.ReportName)));
        }
    }
}