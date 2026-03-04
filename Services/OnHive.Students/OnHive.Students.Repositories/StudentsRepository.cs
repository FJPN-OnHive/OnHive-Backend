using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Payments;
using OnHive.Core.Library.Contracts.Students;
using OnHive.Core.Library.Entities.Students;
using OnHive.Core.Library.Entities.Users;
using OnHive.Core.Library.Helpers;
using OnHive.Database.Library.Models;
using OnHive.Database.Library.MongoDB;
using OnHive.Students.Domain.Abstractions.Repositories;
using OnHive.Students.Domain.Models;
using MongoDB.Driver;

namespace OnHive.Students.Repositories
{
    public class StudentsRepository : MongoRepositoryBase<Student>, IStudentsRepository
    {
        private readonly MongoDBSettings settings;

        public StudentsRepository(MongoDBSettings settings) : base(settings, "Students")
        {
            this.settings = settings;
        }

        public async Task<List<Student>> GetAllByUserIdAsync(string? userId)
        {
            var filter = Builders<Student>.Filter.Eq(s => s.UserId, userId);
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<List<Student>> GetByReportFilterAsync(EnrollmentReportFilter filter, string? tenantId)
        {
            var embeddedObjectFilter = Builders<StudentCourse>.Filter.And(
                Builders<StudentCourse>.Filter.Gte(x => x.StartTime, filter.InitialDate),
                Builders<StudentCourse>.Filter.Lte(x => x.StartTime, filter.FinalDate));

            var dbFilter = Builders<Student>.Filter.Eq(s => s.TenantId, tenantId)
                & Builders<Student>.Filter.ElemMatch(s => s.Courses, embeddedObjectFilter);

            return await collection.Find(dbFilter).ToListAsync();
        }

        public async Task<IEnumerable<SyntheticEnrollmentDto>> GetSyntheticByReportFilterAsync(EnrollmentReportFilter filter, string? tenantId)
        {
            List<string> queryString = [
                            $@"{{$match : {{""TenantId"" : ""{tenantId}"", ""IsActive"" : true}}}}",
                            $@"{{$unwind : ""$Courses"" }}",
                            $@"{{$replaceRoot: {{newRoot: ""$Courses""}}}}",
                            $@"{{$match : {{ ""IsActive"" : true, ""StartTime"" : {{ $gte : new ISODate(""{filter.InitialDate.ToString("yyyy-MM-dd")}""), $lte : new ISODate(""{filter.FinalDate.ToString("yyyy-MM-dd")}"")}}}}}}",
                            $@"{{$group : {{
                                    _id: {{ ""ProductId"" : ""$ProductId"", ""Date"" : {{$dateToString: {{ format: ""%Y-%m-%d"", date: ""$StartTime""}} }} }},
                                    Matriculas: {{ $sum: 1 }},
                                    Certificados : {{
                                        $sum: {{
                                          $cond: {{
                                            if: {{ $eq: [""$CertificateId"", """"] }},
                                            then: 0,
                                            else: 1
                                          }}
                                }}
                              }},
                             }}
                            }}",
                            $@"{{$lookup: {{
                                    from: ""Products"",
                                    localField: ""_id.ProductId"",
                                    foreignField: ""_id"",
                                    as: ""Products""
                                }}
                            }}",
                            $@"{{$unwind : ""$Products""}}",
                            $@"{{ $project : {{
                                    _id: ""$_id.ProductId"",
                                    TenantId : ""{tenantId}"",
                                    ProductSku : ""$Products.Sku"",
                                    ProductName : ""$Products.Name"",
                                    Date: ""$_id.Date"",
                                    Enrollments : ""$Matriculas"",
                                    Certificates : ""$Certificados""
                                }}
                            }}",
                            $@"{{ $sort : {{""Sku"": 1,  ""Data"" : 1}}}}"
                        ];
            PipelineDefinition<Student, SyntheticEnrollmentDto> pipeline = PipelineDefinition<Student, SyntheticEnrollmentDto>.Create(queryString);

            var result = await collection.AggregateAsync(pipeline);
            return result.ToList();
        }

        public async Task<IEnumerable<AnalyticEnrollment>> GetAnalyticByReportFilterAsync(EnrollmentReportFilter filter, string? tenantId)
        {
            var coursesFilter = $@"{{$match : {{ ""Courses.StartTime"" : {{ $gte : new ISODate(""{filter.InitialDate.ToString("yyyy-MM-dd")}""), $lte : new ISODate(""{filter.FinalDate.ToString("yyyy-MM-dd")}"")}}}}}}";

            if (filter.Courses != null && filter.Courses.Any())
            {
                var coursesIds = string.Join(", ", filter.Courses.Select(c => $"\"{c}\""));
                coursesFilter = $@"{{$match : {{ ""Courses.StartTime"" : {{ $gte : new ISODate(""{filter.InitialDate.ToString("yyyy-MM-dd")}""), $lte : new ISODate(""{filter.FinalDate.ToString("yyyy-MM-dd")}"")}}, ""Courses._id"" : {{$in : [{coursesIds}]}}}}}}";
            }

            List<string> queryString = [
                            $@"{{$match : {{""TenantId"" : ""{tenantId}"", ""IsActive"" : true}}}}",
                            $@"{{$unwind : ""$Courses"" }}",
                            coursesFilter,
                            $@"{{$lookup : {{from: ""Users"", localField: ""UserId"", foreignField: ""_id"", as: ""User""}}}}",
                            $@"{{$unwind : ""$User"" }}",
                            $@"{{$lookup : {{from: ""Products"", localField: ""Courses.ProductId"", foreignField: ""_id"", as: ""Product""}}}}",
                            $@"{{$unwind : ""$Product"" }}",
                            $@"{{$project : {{
                                ""UserId"" : ""$User._id"",
                                ""EnrollmentDate"" :   ""$Courses.StartTime"",
                                ""PhoneNumber"" : ""$User.PhoneNumber"",
                                ""CPF"" :  {{ $first : ""$User.Documents.DocumentNumber""}},
                                ""Email"" : {{ $first : ""$User.Emails.Email""}},
                                ""Name"" : {{ $concat : [""$User.Name"", "" "", ""$User.Surname""]}},
                                ""Gender"" : ""$User.Gender"",
                                ""LastAccessedDate"" :  ""$Courses.LastAccessedDate"",
                                ""State"" : {{ $cond : [{{ $eq : [""$Courses.IsActive"",  true]}}, ""Ativa"", ""Inativa""] }},
                                ""ProductName"" : ""$Product.Name"",
                                ""ProductDescription"": ""$Product.Description"",
                                ""EndTime"" :  ""$Courses.EndTime"",
                                ""CertificateDate"" : {{ $cond : [{{$eq : [""$Courses.CertificateId"", """"]}}, ""0001-01-01"", ""$Courses.EndTime""]}},
                                ""Course"" : ""$Courses""
                            }}}}",
                            $@"{{ $sort : {{""EnrollmentDate"" : -1}}}}"
                        ];
            PipelineDefinition<Student, AnalyticEnrollment> pipeline = PipelineDefinition<Student, AnalyticEnrollment>.Create(queryString);

            var result = await collection.AggregateAsync(pipeline);
            return result.ToList();
        }

        public async Task<IEnumerable<AnalyticSurvey>> GetSurveyReportByFilterAsync(EnrollmentReportFilter filter, string? tenantId, string type)
        {
            var coursesFilter = $@"{{$match: {{ ""Courses.Disciplines.Lessons.Type"" : 3, ""Courses.Disciplines.Lessons.State"" : 2,  ""Courses.Disciplines.Lessons.Exam.State"" : 1, ""Courses.Disciplines.Lessons.Exam.SubmitDate"" : {{ $gte: new ISODate(""{filter.InitialDate.ToString("yyyy-MM-dd")}""), $lte: new ISODate(""{filter.FinalDate.ToString("yyyy-MM-dd")}"")}}}}}}";

            if (filter.Courses != null && filter.Courses.Any())
            {
                var coursesIds = string.Join(", ", filter.Courses.Select(c => $"\"{c}\""));
                coursesFilter = $@"{{$match: {{ ""Courses.Disciplines.Lessons.Type"" : 3, ""Courses.Disciplines.Lessons.State"" : 2,  ""Courses.Disciplines.Lessons.Exam.State"" : 1, ""Courses.Disciplines.Lessons.Exam.SubmitDate"" : {{ $gte: new ISODate(""{filter.InitialDate.ToString("yyyy-MM-dd")}""), $lte: new ISODate(""{filter.FinalDate.ToString("yyyy-MM-dd")}"")}}, ""Courses._id"" : {{$in : [{coursesIds}]}}}}}}";
            }

            List<string> queryString =
                [
                $@"{{$match: {{ ""TenantId"" : ""{ tenantId }""}}}}",
                $@"{{$unwind: ""$Courses""}}",
                $@"{{$unwind: ""$Courses.Disciplines""}}",
                $@"{{$unwind: ""$Courses.Disciplines.Lessons""}}",
                $@"{{$unwind: ""$Courses.Disciplines.Lessons.Exam""}}",
                coursesFilter,
                $@"{{$lookup: {{ from: ""Users"", localField: ""UserId"", foreignField: ""_id"", as: ""User""}}}}",
                $@"{{$unwind: ""$User""}}",
                $@"{{$lookup: {{ from: ""Products"", localField: ""Courses.ProductId"", foreignField: ""_id"", as: ""Product""}}}}",
                $@"{{$unwind: ""$Product""}}",
                $@"{{$project:
                    {{
                        ""UserId"" : ""$User._id"",
                        ""EnrollmentDate"" :   ""$Courses.StartTime"",
                        ""PhoneNumber"" : ""$User.PhoneNumber"",
                        ""CPF"" :  {{ $first: ""$User.Documents.DocumentNumber""}},
                        ""Email"" : {{ $first: ""$User.Emails.Email""}},
                        ""Name"" : {{ $concat: [""$User.Name"", "" "", ""$User.Surname""]}},
                        ""Gender"" : ""$User.Gender"",
                        ""ProductCode"" :  ""$Courses.Code"",
                        ""ProductName"" : ""$Product.Name"",
                        ""ProductDescription"" : ""$Product.Description"",
                        ""StudentExam"" : ""$Courses.Disciplines.Lessons.Exam""
                    }}}}",
                $@"{{$lookup: {{ from: ""Exams"", localField: ""StudentExam._id"", foreignField: ""VId"", as: ""Exam""}}}}",
                $@"{{$unwind: ""$Exam""}}",
                $@"{{$match: {{ ""Exam.Name"" : {{$regex: ""{type}""}}}}}}",
                $@"{{ $sort : {{""StudentExam.SubmitDate"" : -1}}}}"
            ];
            PipelineDefinition<Student, AnalyticSurvey> pipeline = PipelineDefinition<Student, AnalyticSurvey>.Create(queryString);
            var result = await collection.AggregateAsync(pipeline);
            return result.ToList();
        }

        public async Task<Student> GetByStudentCodeAsync(string studentCode)
        {
            var filter = Builders<Student>.Filter.Eq(s => s.Code, studentCode);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Student> GetByUserIdAsync(string userId)
        {
            var filter = Builders<Student>.Filter.Eq(s => s.UserId, userId);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        protected override void CreateIndexes()
        {
            collection.Indexes.CreateOne(new CreateIndexModel<Student>(Builders<Student>.IndexKeys.Ascending(i => i.TenantId)));
            collection.Indexes.CreateOne(new CreateIndexModel<Student>(Builders<Student>.IndexKeys.Ascending(i => i.UserId)));
            collection.Indexes.CreateOne(new CreateIndexModel<Student>(Builders<Student>.IndexKeys.Ascending(i => i.Code)));
        }

        public async Task<PaginatedResult<StudentUser>> GetStudentUserByFilter(RequestFilter filter, string? tenantId, bool activeOnly = true)
        {
            var (studentFilter, userFilter) = SplitStudentUserFilters(filter);

            var usersCollection = mongoClient.GetDatabase(settings.DataBase).GetCollection<User>("Users");

            var userQueryFilter = MongoDbFilterConverter.ConvertFilter<User>(userFilter, tenantId, activeOnly);
            FilterDefinition<StudentUser>? studentQueryFilter = null;
            if (HasAnyFilter(studentFilter))
            {
                studentQueryFilter = MongoDbFilterConverter.ConvertFilter<StudentUser>(studentFilter, tenantId, activeOnly);
            }

            var dataQuery = BuildStudentUserAggregate(usersCollection, userQueryFilter, studentQueryFilter);
            var countQuery = BuildStudentUserAggregate(usersCollection, userQueryFilter, studentQueryFilter);

            var countResult = await countQuery.Count().FirstOrDefaultAsync();
            var count = countResult?.Count ?? 0;

            var pageCount = 1L;
            if (filter.Sort.Any())
            {
                dataQuery = dataQuery.Sort(MongoDbFilterConverter.ConvertSort<StudentUser>(filter));
            }
            else
            {
                dataQuery = dataQuery.Sort(Builders<StudentUser>.Sort.Descending(s => s.UpdatedAt));
            }

            if (filter.PageLimit > 0)
            {
                if (filter.Page <= 0)
                {
                    filter.Page = 1;
                }
                dataQuery = dataQuery
                    .Skip((filter.Page - 1) * filter.PageLimit)
                    .Limit(filter.PageLimit);
                pageCount = (long)Math.Ceiling((double)count / filter.PageLimit);
            }

            var items = await dataQuery.ToListAsync();

            return new PaginatedResult<StudentUser>
            {
                Page = filter.Page,
                PageCount = pageCount,
                Total = count,
                Itens = items
            };
        }

        private static (RequestFilter studentFilter, RequestFilter userFilter) SplitStudentUserFilters(RequestFilter filter)
        {
            var studentFilter = new RequestFilter { Type = filter.Type };
            var userFilter = new RequestFilter { Type = filter.Type };

            CopyFilterFields(filter.Filter, studentFilter.Filter, userFilter.Filter);
            CopyFilterFields(filter.AndFilter, studentFilter.AndFilter, userFilter.AndFilter);
            CopyFilterFields(filter.OrFilter, studentFilter.OrFilter, userFilter.OrFilter);

            return (studentFilter, userFilter);
        }

        private static void CopyFilterFields(List<FilterField> source, List<FilterField> studentTarget, List<FilterField> userTarget)
        {
            foreach (var field in source)
            {
                if (IsUserField(field.Field))
                {
                    userTarget.Add(CloneField(field));
                }
                else
                {
                    studentTarget.Add(CloneField(field));
                }
            }
        }

        private static bool IsUserField(string fieldName) =>
            !string.IsNullOrWhiteSpace(fieldName) &&
            fieldName.StartsWith("User.", StringComparison.InvariantCultureIgnoreCase);

        private static FilterField CloneField(FilterField field)
        {
            var normalizedField = field.Field;
            if (normalizedField.Contains('.', StringComparison.InvariantCultureIgnoreCase))
            {
                if (normalizedField.StartsWith("Student.", StringComparison.InvariantCultureIgnoreCase))
                {
                    normalizedField = normalizedField.Substring("Student.".Length);
                }
                else if (normalizedField.StartsWith("User.", StringComparison.InvariantCultureIgnoreCase))
                {
                    normalizedField = normalizedField.Substring("User.".Length);
                    if (normalizedField.Equals("MainEmail", StringComparison.InvariantCultureIgnoreCase))
                    {
                        normalizedField = "Emails.Email";
                    }
                }
            }

            return new FilterField
            {
                Field = string.IsNullOrWhiteSpace(normalizedField) ? field.Field : normalizedField,
                Operator = field.Operator,
                Value = field.Value,
                ValueVariantion = field.ValueVariantion
            };
        }

        private static bool HasAnyFilter(RequestFilter filter) =>
            filter.Filter.Any() || filter.AndFilter.Any() || filter.OrFilter.Any();

        private static IAggregateFluent<StudentUser> BuildStudentUserAggregate(IMongoCollection<User> usersCollection,
            FilterDefinition<User> userFilter,
            FilterDefinition<StudentUser>? studentFilter)
        {
            var aggregate = usersCollection
                .Aggregate()
                .Match(userFilter)
                .Lookup<User, UserStudent>("Students", "Id", "UserId", "Students")
                .Match(u => u.Students != null && u.Students.Any())
                .Project(u => new StudentUser
                {
                    Id = u.Students![0].Id,
                    TenantId = u.Students![0].TenantId,
                    UserId = u.Students![0].UserId,
                    Code = u.Students![0].Code,
                    Version = u.Students![0].Version,
                    VId = u.Students![0].VId,
                    VersionNumber = u.Students![0].VersionNumber,
                    ActiveVersion = u.Students![0].ActiveVersion,
                    IsActive = u.Students![0].IsActive,
                    CreatedAt = u.Students![0].CreatedAt,
                    UpdatedAt = u.Students![0].UpdatedAt,
                    CreatedBy = u.Students![0].CreatedBy,
                    UpdatedBy = u.Students![0].UpdatedBy,
                    Courses = u.Students![0].Courses,
                    User = new User
                    {
                        Id = u.Id,
                        TenantId = u.TenantId,
                        Version = u.Version,
                        VId = u.VId,
                        VersionNumber = u.VersionNumber,
                        ActiveVersion = u.ActiveVersion,
                        Emails = u.Emails,
                        Name = u.Name,
                        Surname = u.Surname,
                        BirthDate = u.BirthDate,
                        Documents = u.Documents,
                        Nationality = u.Nationality,
                        Login = u.Login,
                        PhoneNumber = u.PhoneNumber,
                        Roles = u.Roles,
                        SocialName = u.SocialName,
                        Gender = u.Gender,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt,
                        UpdatedAt = u.UpdatedAt,
                        CreatedBy = u.CreatedBy,
                        UpdatedBy = u.UpdatedBy
                    }
                });

            if (studentFilter != null)
            {
                aggregate = aggregate.Match(studentFilter);
            }

            return aggregate;
        }
    }
}