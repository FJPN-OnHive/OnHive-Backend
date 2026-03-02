using EHive.Configuration.Library.Extensions;
using EHive.Core.Library.Contracts.Events;
using EHive.Events.Api.DependencyInjection;
using EHive.Users.Domain.Models;

namespace EHive.Users.Api.DependencyInjection
{
    public static class BuilderExtensions
    {
        public static WebApplicationBuilder ConfigureUsersApi(this WebApplicationBuilder builder)
        {
            builder.AddConfiguration<UsersApiSettings>();
            builder.Services.AddServices();
            builder.Services.AddRepositories();
            builder.Services.AddMappers();
            builder.ConfigureEventRegister("Users");
            builder.RegisterEvents();
            return builder;
        }

        private static WebApplicationBuilder RegisterEvents(this WebApplicationBuilder builder)
        {
            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.UserCreated,
                Message = "Event triggered when an user is created",
                Origin = "Users",
                Tags = new List<string> { "User", "Created" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" },
                    { "UserPhone", "User Phone" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.UserRegistered,
                Message = "Event triggered when an user is registered with signin",
                Origin = "Users",
                Tags = new List<string> { "User", "Registered", "Created" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" },
                    { "UserPhone", "User Phone" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.UserUpdated,
                Message = "Event triggered when an user is updated",
                Origin = "Users",
                Tags = new List<string> { "User", "Updated" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" },
                    { "UserPhone", "User Phone" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.UserActivated,
                Message = "Event triggered when an user is activated",
                Origin = "Users",
                Tags = new List<string> { "User", "Updated" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" },
                    { "UserPhone", "User Phone" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.UserDeactivated,
                Message = "Event triggered when an user is deactivated",
                Origin = "Users",
                Tags = new List<string> { "User", "Deactivated" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" },
                    { "UserPhone", "User Phone" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.UserRemoved,
                Message = "Event triggered when an user is removed and anonymized",
                Origin = "Users",
                Tags = new List<string> { "User", "Removed" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" },
                    { "UserPhone", "User Phone" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.TempPasswordCreated,
                Message = "Event triggered when a temp password is created",
                Origin = "Users",
                Tags = new List<string> { "User", "TempPasswordCreated" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" },
                    { "UserPhone", "User Phone" }
                }
            });

            builder.ConfigureEvent(new EventMessage
            {
                Key = EventKeys.TempPasswordUsed,
                Message = "Event triggered when a temp password is used",
                Origin = "Users",
                Tags = new List<string> { "User", "TempPasswordUsed" },
                Fields = new Dictionary<string, string> {
                    { "UserId", "The id of the user" },
                    { "UserMainEmail", "Main Email" },
                    { "UserLogin", "User Login" },
                    { "UserName", "User Name" },
                    { "UserPhone", "User Phone" }
                }
            });
            return builder;
        }
    }
}