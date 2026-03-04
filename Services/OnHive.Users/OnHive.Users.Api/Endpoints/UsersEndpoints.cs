using OnHive.Authorization.Library.Extensions;
using OnHive.Configuration.Library.Models;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Entities.Students;
using OnHive.Core.Library.Entities.Teachers;
using OnHive.Core.Library.Enums.Users;
using OnHive.Users.Domain.Abstractions.Services;
using OnHive.WebExtensions.Library;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace OnHive.Users.Api.Endpoints
{
    internal static class UsersEndpoints
    {
        internal static WebApplication MapUsersEndpoints(this WebApplication app)
        {
            app.MapGet("v1/User", async (HttpContext context, [FromServices] IUsersService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) throw new UnauthorizedAccessException();
                var result = await service.GetByIdAsync(loggedUser.User.Id, loggedUser);
                if (result == null) return Results.Ok(Response<UserDto>.Empty("User not found"));
                return Results.Ok(Response<UserDto>.Ok(result));
            })
              .WithName("GetUser")
              .WithDescription("Get user by Token")
              .WithTags("Users")
              .WithMetadata(PermissionConfig.Create("users_read"))
              .Produces<Response<UserDto>>();

            app.MapGet("v1/User/{userId}", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) throw new UnauthorizedAccessException();
                var result = await service.GetByIdAsync(userId, loggedUser);
                if (result == null) return Results.Ok(Response<UserDto>.Empty("User not found"));
                return Results.Ok(Response<UserDto>.Ok(result));
            })
            .WithName("GetUserById")
            .WithDescription("Get user by ID")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_read"))
            .Produces<Response<UserDto>>();

            //app.MapGet("v1/Internal/User/{userId}", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId) =>
            //{
            //    var result = await service.GetByIdAsync(userId);
            //    if (result == null) return Results.Ok(Response<UserDto>.Empty("User not found"));
            //    return Results.Ok(Response<UserDto>.Ok(result));
            //})
            //.WithName("GetUserByIdOpen")
            //.WithDescription("Get user by ID")
            //.WithTags("Internal")
            //.Produces<Response<UserDto>>()
            //.AllowAnonymous();

            app.MapGet("v1/User/ByLogin/{userLogin}", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userLogin) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.GetByLogin(userLogin, loggedUser);
                if (result == null) return Results.Ok(Response<UserDto>.Empty("User not found"));
                return Results.Ok(Response<UserDto>.Ok(result));
            })
            .WithName("GetUserByLogin")
            .WithDescription("Get user by Login")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_read"))
            .Produces<Response<UserDto>>();

            app.MapGet("v1/User/ByEmail/{userEmail}", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userEmail) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.GetByMainEmail(userEmail, loggedUser);
                if (result == null) return Results.Ok(Response<UserDto>.Empty("User not found"));
                return Results.Ok(Response<UserDto>.Ok(result));
            })
            .WithName("GetUserByEmail")
            .WithDescription("Get user by Email")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_read"))
            .Produces<Response<UserDto>>();

            app.MapGet("v1/Users", async (HttpContext context, [FromServices] IUsersService service) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByFilterAsync(filter, loggedUser);
                return Results.Ok(Response<PaginatedResult<UserDto>>.Ok(result, $"Found {result.Itens.Count} users"));
            })
            .WithName("GetUsers")
            .WithDescription("Get all users on this tenant")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_list"))
            .Produces<Response<PaginatedResult<UserDto>>>();

            app.MapPost("v1/Users/ByIds", async (HttpContext context, [FromServices] IUsersService service, [FromBody] List<string> usersIds) =>
            {
                var user = context.GetUser();
                if (user == null) return Results.Unauthorized();
                var filter = context.GetFilter();
                var result = await service.GetByIdsAsync(usersIds, filter, user);
                if (result == null) return Results.Ok(Response<UserDto>.Empty());
                return Results.Ok(Response<PaginatedResult<UserDto>>.Ok(result));
            })
          .WithName("GetUsersByIds")
          .WithDescription("Get Users By Ids")
          .WithTags("Users")
          .WithMetadata(PermissionConfig.Create("users_list"))
          .Produces<Response<PaginatedResult<UserDto>>>();

            app.MapGet("v1/Users/ByProfile/{profile}", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string profile) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var filter = context.GetFilter();

                var profileType = profile.ToLower() switch
                {
                    "staff" => ProfileTypes.Staff,
                    "student" => ProfileTypes.Student,
                    "teacher" => ProfileTypes.Teacher,
                    "monitor" => ProfileTypes.Monitor,
                    "author" => ProfileTypes.Author,
                    _ => throw new ArgumentException("Invalid profile type")
                };
                var result = await service.GetByFilterAndProfileAsync(filter, profileType, loggedUser);
                return Results.Ok(Response<PaginatedResult<UserDto>>.Ok(result, $"Found {result.Itens.Count} users"));
            })
           .WithName("GetUsersByProfile")
           .WithDescription("Get all users By Profile on this tenant")
           .WithTags("Users")
           .WithMetadata(PermissionConfig.Create("users_list"))
           .Produces<Response<PaginatedResult<UserDto>>>();

            app.MapPost("v1/User", async (HttpContext context, [FromServices] IUsersService service, [FromBody] UserDto newUser) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateAsync(newUser, loggedUser);
                if (result == null) return Results.Ok(Response<UserDto>.Empty());
                return Results.Ok(Response<UserDto>.Ok(result));
            })
            .WithName("CreateUser")
            .WithDescription("Create a user")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_create"))
            .Produces<Response<UserDto>>();

            app.MapPost("v1/User/Register", async (HttpContext context, [FromServices] IUsersService service, [FromBody] SignInUserDto newUser) =>
            {
                var result = await service.CreateAsync(newUser);
                return Results.Ok(Response<UserDto>.Ok(result, $"User created, validation code sent to {newUser.Email}"));
            })
            .WithName("UserRegister")
            .WithDescription("Register, create a new user.")
            .WithTags("Users")
            .Produces<Response<UserDto>>()
            .AllowAnonymous();

            app.MapPut("v1/User", async (HttpContext context, [FromServices] IUsersService service, [FromBody] UserDto userDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAsync(userDto, loggedUser);
                if (result == null) return Results.Ok(Response<UserDto>.Empty());
                return Results.Ok(Response<UserDto>.Ok(result));
            })
            .WithName("UpdateUser")
            .WithDescription("Update a user by entity")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_update"))
            .Produces<Response<UserDto>>();

            app.MapPatch("v1/User", async (HttpContext context, [FromServices] IUsersService service, [FromBody] JsonDocument patchDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.PatchAsync(patchDto, loggedUser);
                if (result == null) return Results.Ok(Response<UserDto>.Empty());
                return Results.Ok(Response<UserDto>.Ok(result));
            })
            .WithName("PatchUser")
            .WithDescription("Update a user by patch")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_update"))
            .Produces<Response<UserDto>>();

            app.MapPut("v1/User/Roles", async (HttpContext context, [FromServices] IUsersService service, [FromBody] UserDto userDto) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateRolesAsync(userDto, loggedUser);
                if (result == null) return Results.Ok(Response<UserDto>.Empty());
                return Results.Ok(Response<UserDto>.Ok(result));
            })
            .WithName("UpdateUserRoles")
            .WithDescription("Update a user roles")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_roles_update"))
            .Produces<Response<UserDto>>();

            app.MapPost("v1/User/{userId}/Email", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId, [FromBody] string email) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.AddEmailsAsync(userId, email, loggedUser);
                if (result == false) return Results.Ok(Response<UserDto>.Empty());
                return Results.Ok(Response<UserDto>.Ok());
            })
            .WithName("AddUserEmail")
            .WithDescription("Add a new email to an user")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_update"))
            .Produces<Response<UserDto>>();

            app.MapPut("v1/User/{userId}/Email/SetMain", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId, [FromBody] string email) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.SetMainEmailsAsync(userId, email, loggedUser);
                if (result == false) return Results.Ok(Response<UserDto>.Empty());
                return Results.Ok(Response<UserDto>.Ok());
            })
           .WithName("SetMainUserEmail")
           .WithDescription("Set an existing email as main email")
           .WithTags("Users")
           .WithMetadata(PermissionConfig.Create("users_update"))
           .Produces<Response<UserDto>>();

            app.MapDelete("v1/User/{userId}/Email/{email}", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId, [FromRoute] string email) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.RemoveEmailsAsync(userId, email, loggedUser);
                if (result == false) return Results.Ok(Response<UserDto>.Empty());
                return Results.Ok(Response<UserDto>.Ok());
            })
           .WithName("DeleteUserEmail")
           .WithDescription("Delete an existing email")
           .WithTags("Users")
           .WithMetadata(PermissionConfig.Create("users_update"))
           .Produces<Response<UserDto>>();

            app.MapGet("v1/User/ValidateEmail", async (HttpContext context,
                                                       [FromServices] IUsersService service,
                                                       [FromServices] ILoginService login,
                                                       [FromQuery] string code,
                                                       [FromQuery] string tenantId) =>
            {
                var user = await service.ValidateEmailAsync(code, tenantId);
                if (user == null) return Results.Ok(Response<LoggedUserDto>.Empty("Invalid validation code"));
                var loginResponse = await login.ImpersonateAsync(user.Id);
                if (loginResponse == null) return Results.Ok(Response<LoggedUserDto>.Empty("User not found"));
                return Results.Ok(Response<LoginResponseDto>.Ok(loginResponse, "Email validated"));
            })
            .WithName("ValidateEmail")
            .WithDescription("Validate an email (main)")
            .WithTags("Users")
            .Produces<Response<LoginResponseDto>>()
            .AllowAnonymous();

            app.MapGet("v1/User/ValidateEmail/{tenantId}/{userId}/{email}", async (HttpContext context,
                                                       [FromServices] IUsersService service,
                                                       [FromRoute] string tenantId,
                                                       [FromRoute] string userId,
                                                       [FromRoute] string email) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var user = await service.ValidateEmailAsync(email, userId, tenantId);
                if (user == null) return Results.Ok(Response<UserEmailDto>.Empty("Invalid user"));
                var emailDto = user.Emails.FirstOrDefault(e => e.Email.ToLower() == email.ToLower());
                if (emailDto == null) return Results.Ok(Response<UserEmailDto>.Empty("Invalid email"));
                return Results.Ok(Response<UserEmailDto>.Ok(emailDto, "Email validated"));
            })
            .WithName("ValidateEmailAdmin")
            .WithDescription("Validate an email Admin")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_admin"))
            .Produces<Response<UserEmailDto>>();

            app.MapGet("v1/User/MainEmail/ResendValidationEmail", async (HttpContext context, [FromServices] IUsersService service, [FromQuery] string email, [FromQuery] string tenantId) =>
            {
                await service.ResendMainEmailValidationAsync(email, tenantId);
                return Results.Ok(Response<string>.Ok($"Validation code sent to {email}"));
            })
            .WithName("ResendValidationMainEmail")
            .WithDescription("Validate the main email")
            .WithTags("Users")
            .Produces<Response<string>>()
            .AllowAnonymous();

            app.MapGet("v1/User/SecondaryEmail/ResendValidationEmail", async (HttpContext context, [FromServices] IUsersService service, [FromQuery] string userId, [FromQuery] string email, [FromQuery] string tenantId) =>
            {
                await service.ResendEmailValidationAsync(userId, email, tenantId);
                return Results.Ok(Response<string>.Ok($"Email {email} validated"));
            })
            .WithName("ResendSecondaryValidationSecondaryEmail")
            .WithDescription("Resend the validation code for an secondary email")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_update"))
            .Produces<Response<string>>();

            app.MapPut("v1/User/ChangePassword", async (HttpContext context, [FromServices] IUsersService service, [FromBody] ChangePasswordDto changePassword) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                await service.ChangePasswordAsync(changePassword, loggedUser);
                return Results.Ok(Response<string>.Ok("Password changed"));
            })
             .WithName("ChangePassword")
             .WithDescription("Change password")
             .WithTags("Users")
             .WithMetadata(PermissionConfig.Create("users_update"))
             .Produces<Response<string>>();

            app.MapGet("v1/User/Deactivate/{userId}", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                await service.DeactivateUser(userId, loggedUser);
                return Results.Ok(Response<string>.Ok($"User deactivated"));
            })
            .WithName("DeactivateUser")
            .WithDescription("Deactivate a user")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_admin"))
            .Produces<Response<string>>();

            app.MapGet("v1/User/Reactivate/{userId}", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                await service.ReactivateUser(userId, loggedUser);
                return Results.Ok(Response<string>.Ok($"User reactivated"));
            })
            .WithName("ReactivateUser")
            .WithDescription("Reactivate a user")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_admin"))
            .Produces<Response<string>>();

            app.MapDelete("v1/User/{userId}", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                await service.Anonymize(userId, loggedUser);
                return Results.Ok(Response<string>.Ok($"User deleted"));
            })
            .WithName("DeleteUser")
            .WithDescription("Delete a user")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_admin"))
            .Produces<Response<string>>();

            app.MapDelete("v1/User/AccountData/{userId}", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                await service.DeleteAccountDataAsync(userId, loggedUser);
                return Results.Ok(Response<string>.Ok($"Account Data deleted"));
            })
            .WithName("DeleteUserAcountData")
            .WithDescription("Delete a user account data (addresses, secondary emails, courses)")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_update"))
            .Produces<Response<string>>();

            app.MapPost("v1/User/{userId}/Address", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId, [FromBody] AddressDto address) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.AddAddressAsync(userId, address, loggedUser);
                if (result == null) return Results.Ok(Response<UserDto>.Empty());
                return Results.Ok(Response<UserDto>.Ok(result));
            })
            .WithName("AddUserAddress")
            .WithDescription("Add a new Address to an user")
            .WithTags("Users")
            .WithMetadata(PermissionConfig.Create("users_update"))
            .Produces<Response<UserDto>>();

            app.MapPut("v1/User/{userId}/Address/SetMain/{addressName}", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId, [FromRoute] string addressName) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.SetMainAddressAsync(userId, addressName, loggedUser);
                if (result == null) return Results.Ok(Response<UserDto>.Empty());
                return Results.Ok(Response<UserDto>.Ok(result));
            })
           .WithName("SetMainUserAddress")
           .WithDescription("Set an existing address as main address")
           .WithTags("Users")
           .WithMetadata(PermissionConfig.Create("users_update"))
           .Produces<Response<UserDto>>();

            app.MapPut("v1/User/{userId}/Address", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId, [FromBody] AddressDto address) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.UpdateAddressAsync(userId, address, loggedUser);
                if (result == null) return Results.Ok(Response<UserDto>.Empty());
                return Results.Ok(Response<UserDto>.Ok(result));
            })
           .WithName("UpdateUserAddress")
           .WithDescription("Set an existing address as main address")
           .WithTags("Users")
           .WithMetadata(PermissionConfig.Create("users_update"))
           .Produces<Response<UserDto>>();

            app.MapDelete("v1/User/{userId}/Address/{addressName}", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId, [FromRoute] string addressName) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.RemoveAddressAsync(userId, addressName, loggedUser);
                if (result == null) return Results.Ok(Response<UserDto>.Empty());
                return Results.Ok(Response<UserDto>.Ok(result));
            })
           .WithName("DeleteUserAddress")
           .WithDescription("Delete an existing Address")
           .WithTags("Users")
           .WithMetadata(PermissionConfig.Create("users_update"))
           .Produces<Response<UserDto>>();

            app.MapGet("v1/User/{userId}/Validation/{email}", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId, [FromRoute] string email) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.GetLastValidationCodeAsync(userId, email, loggedUser);
                if (string.IsNullOrEmpty(result)) return Results.Ok(Response<string>.Empty());
                return Results.Ok(Response<string>.Ok(result));
            })
           .WithName("GetLastValidationCode")
           .WithDescription("Get User last validation Code for Email")
           .WithTags("Users")
           .WithMetadata(PermissionConfig.Create("users_admin"))
           .Produces<Response<string>>();

            app.MapGet("v1/User/{userId}/CreateTempPassword", async (HttpContext context, [FromServices] IUsersService service, [FromRoute] string userId) =>
            {
                var loggedUser = context.GetLoggedUser();
                if (loggedUser == null) return Results.Unauthorized();
                var result = await service.CreateTempPasswordAsync(userId, loggedUser);
                if (string.IsNullOrEmpty(result)) return Results.NotFound();
                return Results.Ok(Response<string>.Ok(result, "Password Created"));
            })
           .WithName("CreateTempPassword")
           .WithDescription("Create Temp Password")
           .WithTags("Users")
           .WithMetadata(PermissionConfig.Create("users_admin"))
           .Produces<Response<string>>();

            return app;
        }
    }
}