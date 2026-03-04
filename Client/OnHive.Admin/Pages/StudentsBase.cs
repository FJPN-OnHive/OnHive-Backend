using OnHive.Admin.Base;
using OnHive.Admin.Models;
using OnHive.Admin.Services;
using OnHive.Core.Library.Contracts.Catalog;
using OnHive.Core.Library.Contracts.Certificates;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Courses;
using OnHive.Core.Library.Contracts.Students;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Exceptions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnHive.Admin.Pages
{
    public class StudentsBase : PaginatedComponentBase<StudentDto>
    {
        [Inject] public IAdminService AdminService { get; set; }

        public List<StudentUserDto> Students { get; set; } = [];
        public StudentUserDto? SelectedStudent { get; set; }
        public UserDto? SelectedUser { get; set; }
        public string ProductSearch { get; set; } = string.Empty;
        public bool Loading { get; set; } = true;
        public List<ProductDto> StudentCourses { get; set; } = [];
        public List<ProductDto> Products { get; set; } = [];
        public List<ProductDto> ProductsFiltered { get; set; } = [];
        public bool ShowMessage { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool ShowOnlyEnrolledFilter { get; set; }

        public CertificateMountDto CertificateMount { get; set; } = null;

        private string Token => AdminService?.LoggedUser?.Token ?? string.Empty;

        protected override async Task OnInitializedAsync()
        {
            Loading = true;
            AdminService.Updated += async (s, e) => await InvokeAsync(StateHasChanged);
            await AdminService.VerifyLogin();
            if (!AdminService.IsLoggedIn)
            {
                await AdminService.Logout("/students");
            }
            if (AdminService?.LoggedUser?.User?.IsChangePasswordRequested ?? false)
            {
                await AdminService.ChangePassword("/students");
            }
            await LoadStudents();
            Loading = false;
        }

        private async Task LoadStudents()
        {
            try
            {
                IsLoading = true;
                await LoadProducts();
                await LoadDataAsync();
            }
            catch (NotFoundException)
            {
                Students = [];
                Message = $"Nenhum Aluno encontrado";
                ShowMessage = true;
            }
            catch (Exception ex)
            {
                Message = $"Erro ao carregar os Alunos";
                ShowMessage = true;
                throw;
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected override async Task LoadDataAsync()
        {
            try
            {
                await ApplyStudentSearchFilterAsync();
                var paginatedStudents = await AdminService.StudentsService.GetStudentUsersPaginated(Filter, Token);

                CalculateTotalPages(paginatedStudents.PageCount, paginatedStudents.Total, paginatedStudents.Itens?.Count ?? 0);
                Students = paginatedStudents.Itens ?? [];

                if (SelectedStudent != null)
                {
                    var updatedSelected = Students.FirstOrDefault(s => s.Student.Id == SelectedStudent.Student.Id);
                    if (updatedSelected != null)
                    {
                        SelectedStudent = updatedSelected;
                        SelectedUser = updatedSelected.User;
                        StudentCourses = Products.Where(p => SelectedStudent.Student.Courses.Exists(c => c.Id == p.ItemId)).ToList();
                    }
                    else
                    {
                        SelectedStudent = null;
                        SelectedUser = null;
                        StudentCourses = [];
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/students");
            }
            catch (NotFoundException)
            {
                Students = [];
                TotalPages = 0;
            }
            catch (Exception ex)
            {
                Message = "Erro ao carregar os Alunos";
                ShowMessage = true;
                throw;
            }
        }

        private async Task LoadProducts()
        {
            try
            {
                Products = await AdminService.ProductsService.GetAll(Token);
                ProductsFiltered = Products;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/students");
            }
            catch (Exception ex)
            {
                Message = "Erro ao carregar os Cursos";
                ShowMessage = true;
                throw;
            }
        }

        private async Task LoadUsersForStudentsPage(List<StudentDto> studentsList)
        {
            Students = [];
            var userIds = studentsList.Where(s => !string.IsNullOrWhiteSpace(s.UserId)).Select(s => s.UserId).Distinct().ToList();

            if (!userIds.Any()) return;

            try
            {
                var request = new RequestFilter
                {
                    Page = 1,
                    PageLimit = userIds.Count
                };
                request.Filter.Add(new FilterField
                {
                    Field = "Id",
                    Operator = "in",
                    Value = string.Join(",", userIds)
                });

                var usersResponse = await AdminService.UsersService.GetPaginated(request, Token);
                var usersById = (usersResponse.Itens ?? [])
                    .Where(u => !string.IsNullOrWhiteSpace(u.Id))
                    .ToDictionary(u => u.Id);

                Students = studentsList
                    .Select(student => usersById.TryGetValue(student.UserId, out var user) ? new StudentUserDto() { Student = student, User = user } : null)
                    .Where(dto => dto != null)
                    .Cast<StudentUserDto>()
                    .ToList();
            }
            catch (NotFoundException)
            {
                Students = [];
            }
        }

        public async Task Enroll(ProductDto product)
        {
            if (SelectedUser != null)
            {
                Loading = true;
                var enrolmentMessage = new EnrollmentMessage
                {
                    TenantId = product.TenantId,
                    UserId = SelectedUser.Id,
                    ProductId = product.Id
                };
                var result = await AdminService.StudentsService.EnrollAsync(enrolmentMessage, AdminService?.LoggedUser?.Token ?? string.Empty);
                if (result == null)
                {
                    Message = $"Erro ao inscrever o Aluno";
                    ShowMessage = true;
                }
                else
                {
                    Message = $"Aluno inscrito com sucesso";
                    ShowMessage = true;
                    var existing = Students.Find(s => s.Student.Id == result.Id);
                    if (existing != null)
                    {
                        existing.Student = result;
                        SelectedStudent = existing;
                    }
                    else
                    {
                        SelectedStudent = new StudentUserDto() { Student = result, User = SelectedUser };
                        Students.Add(SelectedStudent);
                    }

                    if (SelectedStudent != null)
                    {
                        var courseIds = SelectedStudent.Student.Courses
                            .Select(c => c.Id)
                            .ToHashSet(StringComparer.InvariantCultureIgnoreCase);
                        StudentCourses = Products.Where(p => courseIds.Contains(p.ItemId)).ToList();
                    }

                    FilterProducts(ShowOnlyEnrolledFilter);
                }
                Loading = false;
                StateHasChanged();
            }
        }

        public void FilterProducts(bool showOnlyEnrolled)
        {
            ShowOnlyEnrolledFilter = showOnlyEnrolled;

            if (showOnlyEnrolled && SelectedStudent != null)
            {
                var enrolledProductIds = SelectedStudent.Student.Courses
                    .Select(c => c.ProductId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToHashSet(StringComparer.InvariantCultureIgnoreCase);

                ProductsFiltered = Products
                    .Where(p => enrolledProductIds.Contains(p.VId))
                    .ToList();
            }
            else
            {
                ProductsFiltered = Products;
            }
            StateHasChanged();
        }

        public async Task Unenroll(ProductDto product)
        {
            if (SelectedStudent != null)
            {
                Loading = true;
                var result = await AdminService.StudentsService.UnenrollAsync(SelectedStudent.User.Id, product.ItemId, AdminService?.LoggedUser?.Token ?? string.Empty);
                if (result == null)
                {
                    Message = $"Erro ao desinscrever o Aluno";
                    ShowMessage = true;
                }
                else
                {
                    Message = $"Aluno desinscrito com sucesso";
                    ShowMessage = true;
                    var existing = Students.Find(s => s.Student.Id == result.Id);
                    if (existing != null)
                    {
                        existing.Student = result;
                        SelectedStudent = existing;
                        StudentCourses = Products.Where(p => SelectedStudent.Student.Courses.Exists(c => c.Id == p.ItemId)).ToList();
                    }

                    if (ShowOnlyEnrolledFilter)
                    {
                        FilterProducts(true);
                    }
                }
                Loading = false;
                StateHasChanged();
            }
        }

        public async Task EmmitCertificate(string courseId)
        {
            if (SelectedStudent != null)
            {
                Loading = true;
                try
                {
                    await AdminService.StudentsService.ReemitCertificateAsync(courseId, SelectedStudent.User.Id, AdminService?.LoggedUser?.Token ?? string.Empty);
                    Message = $"Certificado reemitido com sucesso";
                    ShowMessage = true;
                    SelectedStudent.Student = await AdminService.StudentsService.GetById(SelectedStudent.Student.Id, AdminService?.LoggedUser?.Token ?? string.Empty);
                }
                catch (Exception ex)
                {
                    Message = $"Erro ao reemitir o certificado";
                    ShowMessage = true;
                    throw;
                }
                finally
                {
                    Loading = false;
                    StateHasChanged();
                }
            }
        }

        public async Task ViewCertificate(string courseId)
        {
            if (SelectedStudent != null)
            {
                Loading = true;
                try
                {
                    var StudentCourse = SelectedStudent.Student.Courses.FirstOrDefault(c => c.Id.Equals(courseId, StringComparison.InvariantCultureIgnoreCase));
                    CertificateMount = await AdminService.CertificatesService.GetEmmitedCertificateById(StudentCourse.CertificateId, AdminService?.LoggedUser?.Token ?? string.Empty);
                    if (CertificateMount != null)
                    {
                        Message = $"Certificado recuperado com sucesso";
                        ShowMessage = true;
                    }
                    else
                    {
                        Message = $"Nenhum certificado encontrado.";
                        ShowMessage = true;
                    }
                }
                catch (Exception ex)
                {
                    Message = $"Nenhum certificado encontrado.";
                    ShowMessage = true;
                }
                finally
                {
                    Loading = false;
                    StateHasChanged();
                }
            }
        }

        public async Task SelectUser(UserDto user)
        {
            SelectedUser = user;
            SelectedStudent = Students.FirstOrDefault(s => s.Student.UserId == user.Id);
            if (SelectedStudent == null)
            {
                var student = await GetStudentByUserId(user.Id);
                if (student != null)
                {
                    SelectedStudent = new StudentUserDto() { Student = student, User = user };
                    Students.Add(SelectedStudent);
                }
            }

            StudentCourses = [];
            if (SelectedStudent != null)
            {
                StudentCourses = Products.Where(p => SelectedStudent.Student.Courses.Exists(c => c.Id == p.ItemId)).ToList();
            }

            FilterProducts(false);
            StateHasChanged();
        }

        public bool CheckEnrolment(ProductDto product)
        {
            if (SelectedStudent != null)
            {
                return SelectedStudent.Student.Courses.Exists(c => c.ProductId == product.Id);
            }
            return false;
        }

        public string RowStudentSelector(StudentUserDto item, int rowIndex)
        {
            return (item.Equals(SelectedStudent))
                ? "mud-info"
                : "";
        }

        public string RowUserSelector(UserDto item, int rowIndex)
        {
            return (item.Equals(SelectedUser))
                ? "mud-info"
                : "";
        }

        public string RowUserCourseSelector(CourseDto item, int rowIndex)
        {
            return (item.Equals(SelectedUser))
                ? "mud-info"
                : "";
        }

        private Task ApplyStudentSearchFilterAsync()
        {
            Filter.OrFilter = [];

            var searchTerm = SearchTerm?.Trim();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Task.CompletedTask;
            }
            var fields = new List<string>
            {
                "User.Name",
                "User.Surname",
                "User.SocialName",
                "User.Login",
                "User.MainEmail",
                "User.Emails.Email",
                "User.Documents.DocumentNumber"
            };

            if (LooksLikeNumericCode(searchTerm))
            {
                fields.Add("Student.Code");
            }

            if (LooksLikeGuid(searchTerm))
            {
                fields.Add("Student.UserId");
                fields.Add("User.Id");
            }

            foreach (var field in fields)
            {
                Filter.OrFilter.Add(new FilterField
                {
                    Field = field,
                    Operator = "reg",
                    Value = searchTerm
                });
            }

            return Task.CompletedTask;
        }

        private async Task<StudentDto?> GetStudentByUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            try
            {
                var filter = new RequestFilter
                {
                    Page = 1,
                    PageLimit = 1
                };
                filter.Filter.Add(new FilterField
                {
                    Field = "UserId",
                    Operator = "in",
                    Value = userId
                });

                var paginatedStudents = await AdminService.StudentsService.GetPaginated(filter, AdminService?.LoggedUser?.Token ?? string.Empty);
                return paginatedStudents.Itens?.FirstOrDefault();
            }
            catch (NotFoundException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                await AdminService.Logout("/students");
                return null;
            }
        }

        private static bool LooksLikeNumericCode(string term) =>
            !string.IsNullOrWhiteSpace(term) &&
            term.All(char.IsDigit) &&
            term.Length >= 4;

        private static bool LooksLikeGuid(string term) =>
            !string.IsNullOrWhiteSpace(term) &&
            Guid.TryParse(term, out _);
    }
}