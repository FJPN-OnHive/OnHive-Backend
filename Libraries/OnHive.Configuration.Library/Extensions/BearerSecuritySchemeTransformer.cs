using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace EHive.Configuration.Library.Extensions
{
    internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Insira seu token JWT"
            };

            document.Components ??= new OpenApiComponents();
            if (document.Components.SecuritySchemes == null)
            {
                document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>();
            }
            document.Components.SecuritySchemes.Add("Bearer", securityScheme);

            var requirement = new OpenApiSecurityRequirement();
            requirement.Add(new OpenApiSecuritySchemeReference("Bearer", document), new List<string>());

            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                operation.Value.Security ??= new List<OpenApiSecurityRequirement>();
                operation.Value.Security.Add(requirement);
            }

            return Task.CompletedTask;
        }
    }
}