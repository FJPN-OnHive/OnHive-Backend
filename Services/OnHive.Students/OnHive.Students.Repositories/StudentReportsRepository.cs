using OnHive.Core.Library.Entities.Students;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Students.Domain.Abstractions.Repositories;
using MongoDB.Driver;

namespace OnHive.Students.Repositories
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