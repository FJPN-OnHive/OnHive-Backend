namespace EHive.Posts.Domain.Models
{
    public class PostsApiSettings
    {
        public string? PostsAdminPermission { get; set; } = "posts_admin";

        public string HouseKeepingCron { get; set; } = "0 0 * * *";

        public string ScheduleCron { get; set; } = "* * * * *";

        public int HouseKeepingOlderThanDays { get; set; } = 7;
    }
}