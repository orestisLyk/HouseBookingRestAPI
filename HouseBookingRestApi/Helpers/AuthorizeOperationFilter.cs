using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HouseBookingRestApi.Helpers
{
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var authAttributes = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
                .Distinct();

            if(authAttributes.Any())
            {
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

                // Add security requirements
                operation.Security = new List<OpenApiSecurityRequirement>();

                var roles = context.MethodInfo.GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Where(attr => !string.IsNullOrEmpty(attr.Roles))
                    .SelectMany(attr => attr.Roles!.Split(','))
                    .Select(r => r.Trim());

                // Add the security requirement for the JWT Bearer scheme with specified roles
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Description = "Add token to header",
                            Name = "Authorization",
                            Type = SecuritySchemeType.Http,
                            In = ParameterLocation.Header,
                            Scheme = JwtBearerDefaults.AuthenticationScheme,
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        roles.ToList()
                    }
                });


            }
        }
    }
}
