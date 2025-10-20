using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HRMarket.Configuration.Swagger;

/// <summary>
/// Adds Accept-Language header parameter to all Swagger operations
/// </summary>
public class AcceptLanguageHeaderParameter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Accept-Language",
            In = ParameterLocation.Header,
            Description = @"Language preference for validation messages and error responses.
            
**Supported languages:**
- `ro` or `ro-RO` - Romanian (default)
- `en` or `en-US` - English

If not specified, Romanian will be used by default.",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Enum = new List<IOpenApiAny>
                {
                    new OpenApiString("ro"),
                    new OpenApiString("ro-RO"),
                    new OpenApiString("en"),
                    new OpenApiString("en-US")
                },
                Default = new OpenApiString("ro")
            },
            Example = new OpenApiString("en")
        });
    }
}