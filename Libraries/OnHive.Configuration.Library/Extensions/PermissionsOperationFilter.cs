using EHive.Configuration.Library.Models;
using Microsoft.OpenApi;

using Microsoft.OpenApi;

using Microsoft.AspNetCore.OpenApi;

namespace EHive.Configuration.Library.Extensions
{
    public class PermissionsOperationFilter : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var endpointMetadata = context.Description.ActionDescriptor.EndpointMetadata;
            var permissionsSettings = endpointMetadata.OfType<PermissionConfig>().FirstOrDefault();

            if (permissionsSettings == null)
            {
                return Task.CompletedTask;
            }

            foreach (var permission in permissionsSettings.Permissions ?? Enumerable.Empty<string>())
            {
                PermissionsStore.AddPermission(permission, operation.Description ?? string.Empty);
            }

            operation.Description = $"{operation.Description}<br/><b>Permissions:</b> {(permissionsSettings.Permissions != null && permissionsSettings.Permissions.Any() ? string.Join(", ", permissionsSettings.Permissions) : "None")}<br/><b>Allow Unvalidated Email:</b> {permissionsSettings.AllowUnvalidatedEmail}";

            return Task.CompletedTask;
        }
    }
}