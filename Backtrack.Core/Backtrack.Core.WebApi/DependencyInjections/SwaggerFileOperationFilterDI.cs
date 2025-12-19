using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Backtrack.Core.WebApi.DependencyInjections
{
    public class SwaggerFileOperationFilterDI : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileUploadMime = "multipart/form-data";

            if (operation.RequestBody != null &&
                operation.RequestBody.Content.Any(x => x.Key.Contains(fileUploadMime)))
            {
                // Clear existing parameters
                operation.Parameters.Clear();

                // For multipart/form-data, Swagger UI will automatically show form fields
                // based on the request body schema. We don't need to create individual parameters.

                // Update request body to indicate multipart/form-data
                if (operation.RequestBody != null && operation.RequestBody.Content != null)
                {
                    if (operation.RequestBody.Content.ContainsKey("application/json"))
                    {
                        operation.RequestBody.Content.Remove("application/json");
                    }

                    if (!operation.RequestBody.Content.ContainsKey(fileUploadMime))
                    {
                        operation.RequestBody.Content[fileUploadMime] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = GetFormSchemaProperties(context)
                            }
                        };
                    }
                }
            }
        }

        private Dictionary<string, OpenApiSchema> GetFormSchemaProperties(OperationFilterContext context)
        {
            var properties = new Dictionary<string, OpenApiSchema>();

            var parameters = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType.IsClass && !p.ParameterType.Namespace?.StartsWith("Microsoft.AspNetCore") == true);

            foreach (var param in parameters)
            {
                var paramProperties = param.ParameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in paramProperties)
                {
                    if (property.PropertyType.IsAssignableTo(typeof(IFormFile)))
                    {
                        // Handle file uploads
                        properties[property.Name] = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary",
                            Description = "Upload a PDF file (max 10MB)"
                        };
                    }
                    else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        // Handle nested objects (like jobDescription)
                        var nestedProperties = property.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var nestedProp in nestedProperties)
                        {
                            properties[$"{property.Name}.{nestedProp.Name}"] = new OpenApiSchema
                            {
                                Type = GetPropertyType(nestedProp.PropertyType),
                                Description = GetPropertyDescription(nestedProp)
                            };
                        }
                    }
                    else
                    {
                        // Handle simple properties
                        properties[property.Name] = new OpenApiSchema
                        {
                            Type = GetPropertyType(property.PropertyType),
                            Description = GetPropertyDescription(property)
                        };
                    }
                }
            }

            return properties;
        }

        private string GetPropertyType(Type type)
        {
            if (type == typeof(string))
                return "string";
            if (type == typeof(int) || type == typeof(int?))
                return "integer";
            if (type == typeof(bool) || type == typeof(bool?))
                return "boolean";
            if (type == typeof(Guid) || type == typeof(Guid?))
                return "string";
            if (type == typeof(DateTime) || type == typeof(DateTime?))
                return "string";
            if (type == typeof(decimal) || type == typeof(decimal?))
                return "number";

            return "string";
        }

        private string? GetPropertyDescription(PropertyInfo property)
        {
            // You can add custom logic here to get descriptions from attributes
            // For now, return null and let Swagger generate default descriptions
            return null;
        }
    }
}
