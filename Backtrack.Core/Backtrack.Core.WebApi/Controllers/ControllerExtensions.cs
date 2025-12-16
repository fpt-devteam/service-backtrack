using Backtrack.Core.Contract.Common;
using Backtrack.Core.WebApi.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

public static class ControllerExtensions
{
    private static string GetCorrelationId(ControllerBase controller)
        => controller.HttpContext.Request.Headers.TryGetValue(HeaderNames.CorrelationId, out var values)
            ? values.ToString()
            : string.Empty;

    public static IActionResult ApiOk<T>(this ControllerBase controller, T data)
    {
        var response = ApiResponse<T>.SuccessResponse(data, GetCorrelationId(controller));
        return controller.Ok(response);
    }

    public static IActionResult ApiCreated<T>(this ControllerBase controller, T data)
    {
        var response = ApiResponse<T>.SuccessResponse(data, GetCorrelationId(controller));
        return controller.StatusCode(StatusCodes.Status201Created, response);
    }
}
