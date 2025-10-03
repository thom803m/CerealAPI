using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace CerealAPI.Swagger
{
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Find Authorize attributter på controller og metode
            var authorizeAttrs = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                                 .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
                                 .ToList();

            authorizeAttrs.AddRange(context.MethodInfo.GetCustomAttributes(true)
                                 .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());

            if (!authorizeAttrs.Any()) 
                return; // Ingen Authorize → ingen lås i Swagger

            // Tilføj JWT security definition
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new List<string>()
                    }
                }
            };

            // Tilføj rollebeskrivelse i Swagger
            var roles = authorizeAttrs.Where(a => !string.IsNullOrEmpty(a.Roles))
                                      .Select(a => a.Roles)
                                      .Distinct();

            if (roles.Any())
            {
                operation.Description += $"<br/><b>Kræver rolle:</b> {string.Join(", ", roles)}";
            }
        }
    }
}