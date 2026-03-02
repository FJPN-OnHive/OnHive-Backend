using AutoMapper;
using EHive.Core.Library.Contracts.Posts;
using EHive.Core.Library.Validations.Common;
using EHive.Core.Library.Entities.Posts;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Users;
using EHive.Posts.Domain.Abstractions.Repositories;
using EHive.Posts.Domain.Abstractions.Services;
using EHive.Posts.Domain.Models;
using Serilog;
using EHive.Core.Library.Exceptions;
using EHive.Core.Library.Enums.Posts;
using System.Text.Json;
using EHive.Core.Library.Helpers;
using EHive.Core.Library.Enums.Common;
using EHive.Core.Library.Extensions;
using System.Text;
using EHive.Students.Domain.Abstractions.Services;

namespace EHive.Posts.Services
{
    public class PostsService : IPostsService
    {
        private readonly IPostsRepository postsRepository;
        private readonly IPostBackupRepository backupRepository;
        private IStudentsService studentsService;
        private readonly PostsApiSettings postsApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public PostsService(IPostsRepository postsRepository,
                            IPostBackupRepository backupRepository,
                            PostsApiSettings postsApiSettings,
                            IMapper mapper,
                            IStudentsService studentsService)
        {
            this.postsRepository = postsRepository;
            this.postsApiSettings = postsApiSettings;
            this.studentsService = studentsService;
            this.backupRepository = backupRepository;
            this.mapper = mapper;
            logger = Log.Logger;
        }

        public async Task<string> HouseKeeping()
        {
            var result = await postsRepository.DeleteUnsaved(DateTime.UtcNow.AddDays(postsApiSettings.HouseKeepingOlderThanDays));
            return $"Deleted {result} unsaved posts";
        }

        public async Task<string> ScheduleExecute()
        {
            var result = string.Empty;
            var readyToPublish = await postsRepository.GetReadyToPublish();
            logger.Information("Found {count} posts ready to publish", readyToPublish.Count);
            result += $"Found {readyToPublish.Count} posts ready to publish\n";
            foreach (var post in readyToPublish)
            {
                logger.Information("Publishing post {slug}", post.Slug);
                result += $"Publishing post {post.Slug}\n";
                post.Status = PostStatus.Published;
                await postsRepository.SaveAsync(post);
            }
            return result;
        }

        public async Task<BlogPostDto?> GetByIdAsync(string blogId)
        {
            var blog = await postsRepository.GetByIdAsync(blogId);
            return mapper.Map<BlogPostDto>(blog);
        }

        public async Task<BlogPostDto?> GetBySlugAsync(string slug, string tenantId)
        {
            var blog = await postsRepository.GetBySlug(slug, tenantId);
            if (blog == null)
            {
                blog = await postsRepository.GetByAlternativeSlug(slug, tenantId);
                if (blog == null)
                {
                    return null;
                }
                throw new RedirectException("Post", string.Empty, blog.Slug, "301");
            }
            return mapper.Map<BlogPostDto>(blog);
        }

        public async Task<PaginatedResult<BlogPostDto>> GetByFilterAsync(RequestFilter filter, string tenantId)
        {
            var result = await postsRepository.GetPublishedByFilterAsync(filter, tenantId, false);
            if (result != null)
            {
                return new PaginatedResult<BlogPostDto>
                {
                    Page = result.Page,
                    PageCount = result.PageCount,
                    Total = result.Total,
                    Itens = mapper.Map<List<BlogPostDto>>(result.Itens)
                };
            }
            return new PaginatedResult<BlogPostDto>
            {
                Page = 0,
                PageCount = 0,
                Total = 0,
                Itens = new List<BlogPostDto>()
            };
        }

        public async Task<Dictionary<string, string>> GetCanonicalAsync(string tenantId)
        {
            var result = await postsRepository.GetPublishedCanonicalListAsync(tenantId);
            if (result != null)
            {
                return result.Select(result => new KeyValuePair<string, string>(result.CanonicalUrl ?? string.Empty, result.Slug)).ToDictionary(k => k.Key, v => v.Value);
            }
            return new Dictionary<string, string>();
        }

        public async Task<PaginatedResult<BlogPostDto>> GetByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var blogPosts = new PaginatedResult<BlogPost>();
            if (loggedUser?.User?.Permissions.Exists(p => p.Equals("posts_create", StringComparison.InvariantCultureIgnoreCase)) ?? false)
            {
                blogPosts = await postsRepository.GetByFilterAsync(filter, loggedUser?.User?.TenantId, false);
            }
            else
            {
                blogPosts = await postsRepository.GetPublishedByFilterAsync(filter, loggedUser?.User?.TenantId, false);
            }
            if (blogPosts != null)
            {
                var result = new PaginatedResult<BlogPostDto>
                {
                    Page = blogPosts.Page,
                    PageCount = blogPosts.PageCount,
                    Itens = mapper.Map<List<BlogPostDto>>(blogPosts.Itens)
                };
                result.Itens.ForEach(i => i.LikedByCurrentUser = blogPosts.Itens.Find(b => b.Id == i.Id)?.Likes.Exists(l => l.UserId == loggedUser?.User?.Id) ?? false);
                return result;
            }
            return new PaginatedResult<BlogPostDto>
            {
                Page = 0,
                PageCount = 0,
                Itens = new List<BlogPostDto>()
            };
        }

        public async Task<PaginatedResult<BlogPostDto>> GetResumeByFilterAsync(RequestFilter filter, string tenantId)
        {
            var result = await GetByFilterAsync(filter, tenantId);
            result.Itens.ForEach(i => i.Body = string.Empty);
            return result;
        }

        public async Task<PaginatedResult<BlogPostDto>> GetResumeByFilterAsync(RequestFilter filter, LoggedUserDto? loggedUser)
        {
            var result = await GetByFilterAsync(filter, loggedUser);
            result.Itens.ForEach(i => i.Body = string.Empty);
            return result;
        }

        public async Task<List<BlogPostDto>> GetByCourseAsync(string courseId, LoggedUserDto? loggedUser)
        {
            if (!(await VerifyEnrollment(courseId, loggedUser)))
            {
                throw new UnauthorizedAccessException("User not enrolled");
            }
            var blogs = await postsRepository.GetPublishedByCourseAsync(courseId, loggedUser?.User?.TenantId);
            return mapper.Map<List<BlogPostDto>>(blogs);
        }

        public async Task<IEnumerable<BlogPostDto>> GetAllAsync(LoggedUserDto? loggedUser)
        {
            var blogs = await postsRepository.GetAllAsync(loggedUser?.User?.TenantId);
            return mapper.Map<IEnumerable<BlogPostDto>>(blogs);
        }

        public async Task<BlogPostDto> SaveAsync(BlogPostDto BlogPostDto, LoggedUserDto? loggedUser)
        {
            var blog = mapper.Map<BlogPost>(BlogPostDto);
            ValidatePermissions(blog, loggedUser?.User);
            await validateDuplicate(BlogPostDto, loggedUser?.User?.TenantId);
            blog.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            blog.CreatedAt = DateTime.UtcNow;
            blog.CreatedBy = string.IsNullOrEmpty(blog.CreatedBy) ? loggedUser?.User?.Id : blog.CreatedBy;
            var response = await postsRepository.SaveAsync(blog);
            return mapper.Map<BlogPostDto>(response);
        }

        public async Task<BlogPostDto> CreateAsync(BlogPostDto blogPostDto, LoggedUserDto? loggedUser)
        {
            if (!blogPostDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var newPost = mapper.Map<BlogPost>(blogPostDto);
            ValidatePermissions(newPost, loggedUser?.User);
            await validateDuplicate(blogPostDto, loggedUser?.User?.TenantId);
            newPost.Id = string.Empty;
            newPost.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            newPost = ValidateStatus(blogPostDto, newPost);
            var response = await postsRepository.SaveAsync(newPost, loggedUser.User.Id);
            return mapper.Map<BlogPostDto>(response);
        }

        public async Task<BlogPostDto> CreateDraftAsync(BlogPostDto blogPostDto, LoggedUserDto loggedUser)
        {
            if (string.IsNullOrEmpty(blogPostDto.Slug))
            {
                throw new ArgumentException("Slug it's missing");
            }
            var blog = mapper.Map<BlogPost>(blogPostDto);
            blog.Status = PostStatus.Draft;
            blog.PublishDate = DateTime.MinValue;
            ValidatePermissions(blog, loggedUser?.User);
            await validateDuplicate(blogPostDto, loggedUser?.User?.TenantId);
            blog.Id = string.Empty;
            blog.TenantId = loggedUser?.User?.TenantId ?? throw new ArgumentException(nameof(loggedUser));
            var response = await postsRepository.SaveAsync(blog, loggedUser.User.Id);
            return mapper.Map<BlogPostDto>(response);
        }

        public async Task<BlogPostDto?> UpdateAsync(BlogPostDto blogPostDto, LoggedUserDto? loggedUser)
        {
            if (!blogPostDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            var newPost = mapper.Map<BlogPost>(blogPostDto);
            ValidatePermissions(newPost, loggedUser?.User);
            await validateDuplicate(blogPostDto, loggedUser?.User?.TenantId);
            var currentPost = await postsRepository.GetByIdAsync(newPost.Id);
            if (currentPost == null || currentPost.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            if (!currentPost.Body.Equals(newPost.Body))
            {
                await BackupPost(currentPost, loggedUser.User.Id);
            }
            newPost = ValidateStatus(blogPostDto, newPost, currentPost.Status, currentPost.PublishDate);
            var response = await postsRepository.SaveAsync(newPost, loggedUser.User.Id);
            return mapper.Map<BlogPostDto>(response);
        }

        public async Task<BlogPostDto> PatchAsync(JsonDocument blogPostPatch, LoggedUserDto loggedUser)
        {
            var currentPost = await postsRepository.GetByIdAsync(blogPostPatch.GetId());
            if (currentPost == null || currentPost.TenantId != loggedUser?.User?.TenantId)
            {
                return null;
            }
            var lastStatus = currentPost.Status;
            var lastDate = DateTime.Parse(currentPost.PublishDate.ToString());
            var newPost = blogPostPatch.PatchEntity(currentPost);
            var blogPostDto = mapper.Map<BlogPostDto>(newPost);
            if (!blogPostDto.Validate(out var validationResult))
            {
                throw new ArgumentException(string.Join(", ", validationResult));
            }
            ValidatePermissions(newPost, loggedUser?.User);
            await validateDuplicate(blogPostDto, loggedUser?.User?.TenantId);
            newPost = ValidateStatus(blogPostDto, newPost, lastStatus, lastDate);
            if (!currentPost.Body.Equals(newPost.Body))
            {
                await BackupPost(currentPost, loggedUser.User.Id);
            }
            var response = await postsRepository.SaveAsync(newPost, loggedUser.User.Id);
            return mapper.Map<BlogPostDto>(response);
        }

        public async Task<bool> LikeAsync(string blogId, LoggedUserDto? loggedUser)
        {
            var blog = await postsRepository.GetByIdAsync(blogId);
            if (blog == null)
            {
                return false;
            }
            if (blog.Likes.Exists(l => l.UserId == loggedUser?.User?.Id))
            {
                return true;
            }
            blog.Likes.Add(new BlogPostLike { UserId = loggedUser?.User?.Id ?? string.Empty, CreatedAt = DateTime.UtcNow, Hash = string.Empty });
            await postsRepository.SaveAsync(blog, loggedUser.User.Id);
            return true;
        }

        public async Task<bool> UnlikeAsync(string blogId, LoggedUserDto? loggedUser)
        {
            var blog = await postsRepository.GetByIdAsync(blogId);
            if (blog == null)
            {
                return false;
            }
            if (!blog.Likes.Exists(l => l.UserId == loggedUser?.User?.Id))
            {
                return true;
            }
            blog.Likes.Remove(blog.Likes.Find(l => l.UserId == loggedUser.User.Id));
            await postsRepository.SaveAsync(blog, loggedUser.User.Id);
            return true;
        }

        public async Task<bool> DeleteByIdAsync(string blogId, LoggedUserDto loggedUser)
        {
            var blog = await postsRepository.GetByIdAsync(blogId);
            if (blog == null)
            {
                return false;
            }
            ValidatePermissions(blog, loggedUser?.User);
            await postsRepository.DeleteAsync(blog.Id);
            return true;
        }

        public Task<FilterScope> GetFilterScopeAsync(string tenantId)
        {
            return postsRepository.GetFilterDataAsync(tenantId);
        }

        public Task<List<string>> GetSlugsAsync(string tenantId)
        {
            return postsRepository.GetSlugsAsync(tenantId);
        }

        public async Task<Stream> GetExportData(ExportFormats format, string tenantId, bool activeOnly)
        {
            var posts = activeOnly
               ? await postsRepository.GetAllActive(tenantId)
               : await postsRepository.GetAllAsync(tenantId);

            var stream = format switch
            {
                ExportFormats.Csv => ToCsvStream(posts),
                ExportFormats.Json => ToJsonStream(posts),
                ExportFormats.Xml => ToXmlStream(posts),
                _ => throw new NotImplementedException()
            };
            return stream;
        }

        private async Task BackupPost(BlogPost? currentPost, string userId)
        {
            var backup = mapper.Map<BlogPostBackup>(currentPost);
            backup.Id = string.Empty;
            backup.PostId = currentPost.Id;
            backup.SnapShotDate = DateTime.UtcNow;
            await backupRepository.SaveAsync(backup, userId);
        }

        private Stream ToXmlStream(List<BlogPost> posts)
        {
            var resultXml = $@"<?xml version=""1.0""?>
                               <posts>
                                   {GetXmlItens(posts)}
                               </posts>";
            return new MemoryStream(Encoding.UTF8.GetBytes(resultXml));
        }

        private string GetXmlItens(List<BlogPost> posts)
        {
            var result = "";
            foreach (var post in posts)
            {
                var state = post.Status switch
                {
                    PostStatus.Published => "published",
                    PostStatus.Scheduled => "scheduled",
                    PostStatus.Unpublished => "unpublished",
                    PostStatus.Draft => "draft",
                    PostStatus.Archived => "archived",
                    _ => "unknown"
                };
                result += @$"<post>
                                <id>{post.Id.EscapeXml()}</id>
                                <title>{post.Title.EscapeXml()}</title>
                                <description>{post.Description.EscapeXml()}</description>
                                <imageUrl>{post.CoverImage.EscapeXml()}</imageUrl>
                                <publishDate>{post.PublishDate.ToString("s").EscapeXml()}</publishDate>
                                <author>{post.Author.EscapeXml()}</author>
                                <state>{state}</state>
                                <slug>{post.Slug.EscapeXml()}</slug>
                            </post>
                            ";
            }
            return result;
        }

        private Stream ToJsonStream(List<BlogPost> posts)
        {
            var result = JsonSerializer.Serialize(posts);

            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        private Stream ToCsvStream(List<BlogPost> posts)
        {
            var result = $"id;title;description;imageUrl;publishDate;author;state;slug;categories;breadcrumb\n";
            foreach (var post in posts)
            {
                var state = post.Status switch
                {
                    PostStatus.Published => "published",
                    PostStatus.Scheduled => "scheduled",
                    PostStatus.Unpublished => "unpublished",
                    PostStatus.Draft => "draft",
                    PostStatus.Archived => "archived",
                    _ => "unknown"
                };
                var categories = string.Join(",", post.Categories);
                var author = post.Author != null ? post.Author.Replace(";", " ") : "";
                result += $"{post.Id.Replace(";", " ")};{post.Title.Replace(";", " ")};{post.Description.Replace(";", " ")};{post.CoverImage.Replace(";", " ")};{post.PublishDate.ToString("s").Replace(";", " ")};{author};{state};{post.Slug.Replace(";", " ")};{categories};{post.BreadcrumbTitle}\n";
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        private static BlogPost ValidateStatus(BlogPostDto blogPostDto, BlogPost newPost)
        {
            return ValidateStatus(blogPostDto, newPost, PostStatus.Draft, DateTime.MinValue);
        }

        private static BlogPost ValidateStatus(BlogPostDto blogPostDto, BlogPost newPost, PostStatus currentPostStatus, DateTime currentPostPublishDate)
        {
            if (currentPostStatus != newPost.Status || newPost.PublishDate > currentPostPublishDate)
            {
                switch (newPost.Status)
                {
                    case PostStatus.Published:
                    case PostStatus.Scheduled:
                        if (currentPostStatus != PostStatus.Published)
                        {
                            if ((blogPostDto.PublishDate == null || blogPostDto.PublishDate <= DateTime.UtcNow)
                                || (blogPostDto.Status == PostStatus.Published && currentPostStatus == PostStatus.Scheduled))
                            {
                                newPost.PublishDate = DateTime.UtcNow;
                                newPost.Status = PostStatus.Published;
                            }
                            else
                            {
                                newPost.Status = PostStatus.Scheduled;
                            }
                        }
                        else if (currentPostStatus == newPost.Status && newPost.PublishDate < DateTime.UtcNow)
                        {
                            break;
                        }
                        else
                        {
                            throw new ArgumentException("Post already published");
                        }
                        break;

                    case PostStatus.Unpublished:
                        if (currentPostStatus != PostStatus.Draft && currentPostStatus != PostStatus.Unpublished)
                        {
                            throw new ArgumentException("Cannot change a post to Unpublished after published or scheduled");
                        }
                        break;

                    case PostStatus.Draft:
                        if (currentPostStatus != PostStatus.Draft)
                        {
                            throw new ArgumentException("Cannot change a post to draft");
                        }
                        break;

                    case PostStatus.Archived:
                        if (currentPostStatus != PostStatus.Published && currentPostStatus != PostStatus.Scheduled)
                        {
                            throw new ArgumentException("Cannot archive a post that is not published or scheduled");
                        }
                        break;
                }
            }
            return newPost;
        }

        private async Task<bool> VerifyEnrollment(string courseId, LoggedUserDto loggedUser)
        {
            var isEnrolled = await studentsService.ValidateEnrollment(courseId, loggedUser);
            return isEnrolled != null;
        }

        private async Task validateDuplicate(BlogPostDto blogPostDto, string? tenantId)
        {
            var current = await postsRepository.GetBySlug(blogPostDto.Slug, tenantId);
            if (current != null && current.Id != blogPostDto.Id)
            {
                throw new DuplicatedException("Slug already exists");
            }
        }

        private void ValidatePermissions(BlogPost blog, UserDto? loggedUser)
        {
            if (loggedUser != null && blog.TenantId != loggedUser.TenantId)
            {
                logger.Warning("Unauthorized update mismatch tenantID Post/tenant: {id} / {userTenant}, logged user / tenant: {loggedUserId} / {loggedUserTEnant}",
                    blog.Id, blog.TenantId, loggedUser.Id, loggedUser.TenantId);
                throw new UnauthorizedAccessException();
            }
        }
    }
}