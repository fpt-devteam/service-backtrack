using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Extensions
{
    public static class ModelStateExtensions
    {
        public static IServiceCollection AddConfiguredModelState(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return new BadRequestObjectResult(new
                    {
                        code = "ValidationError",
                        message = "One or more validation errors occurred.",
                        errors,
                        traceId = context.HttpContext.TraceIdentifier
                    });
                };
            });

            return services;
        }
    }
}
