using MongoDB.Driver;
using OnHive.Core.Library.Entities.Students;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Students.Domain.Abstractions.Repositories;

namespace OnHive.Students.Repositories
{
    public class StudentActivitiesRepository : MongoRepositoryBase<StudentActivity>, IStudentActivitiesRepository
    {
        public StudentActivitiesRepository(MongoDBSettings settings) : base(settings, "StudentsActivities")
        {
        }

        public async Task<List<StudentActivity>> GetAllByUserIdAsync(string? userId)
        {
            var filter = Builders<StudentActivity>.Filter.Eq(s => s.UserId, userId);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<List<StudentActivity>> GetByCourseIdAsync(string courseId, string tenantID)
        {
            var filter = Builders<StudentActivity>.Filter.And(
                Builders<StudentActivity>.Filter.Eq(s => s.CourseId, courseId),
                Builders<StudentActivity>.Filter.Eq(s => s.TenantId, tenantID));
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<List<StudentActivity>> GetByCourseandLessonIdAsync(string courseId, string LessonId, string tenantID)
        {
            var filter = Builders<StudentActivity>.Filter.And(
                Builders<StudentActivity>.Filter.Eq(s => s.CourseId, courseId),
                Builders<StudentActivity>.Filter.Eq(s => s.LessonId, LessonId),
                Builders<StudentActivity>.Filter.Eq(s => s.TenantId, tenantID));
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<List<StudentActivity>> GetByCourseIdUserAsync(string userId, string courseId, string tenantID)
        {
            var filter = Builders<StudentActivity>.Filter.And(
                Builders<StudentActivity>.Filter.Eq(s => s.CourseId, courseId),
                Builders<StudentActivity>.Filter.Eq(s => s.TenantId, tenantID),
                Builders<StudentActivity>.Filter.Eq(s => s.UserId, userId));
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<List<StudentActivity>> GetByCourseandLessonIdUserAsync(string userId, string courseId, string LessonId, string tenantID)
        {
            var filter = Builders<StudentActivity>.Filter.And(
                Builders<StudentActivity>.Filter.Eq(s => s.CourseId, courseId),
                Builders<StudentActivity>.Filter.Eq(s => s.LessonId, LessonId),
                Builders<StudentActivity>.Filter.Eq(s => s.TenantId, tenantID),
                Builders<StudentActivity>.Filter.Eq(s => s.UserId, userId));
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<StudentActivity> GetByUserIdAsync(string userId)
        {
            var filter = Builders<StudentActivity>.Filter.Eq(s => s.UserId, userId);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<StudentActivity>(Builders<StudentActivity>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<StudentActivity>(Builders<StudentActivity>.IndexKeys.Ascending(i => i.UserId)));
            collection.Indexes.CreateOne(new CreateIndexModel<StudentActivity>(Builders<StudentActivity>.IndexKeys.Ascending(i => i.EventType)));
            collection.Indexes.CreateOne(new CreateIndexModel<StudentActivity>(Builders<StudentActivity>.IndexKeys.Ascending(i => i.CourseId)));
            collection.Indexes.CreateOne(new CreateIndexModel<StudentActivity>(Builders<StudentActivity>.IndexKeys.Ascending(i => i.LessonId)));
        }
    }
}