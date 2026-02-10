
using Backtrack.Core.WebApi.Common;
using Backtrack.Core.WebApi.Constants;
using Backtrack.Core.WebApi.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

public static class ControllerExtensions
{
    public static IActionResult ApiOk<T>(this ControllerBase controller, T data)
    {
        var response = ApiResponse<T>.SuccessResponse(data, HttpContextUtil.GetHeaderValue(controller.HttpContext, HeaderNames.CorrelationId));
        return controller.Ok(response);
    }

    public static IActionResult ApiCreated<T>(this ControllerBase controller, T data)
    {
        var response = ApiResponse<T>.SuccessResponse(data, HttpContextUtil.GetHeaderValue(controller.HttpContext, HeaderNames.CorrelationId));
        return controller.StatusCode(StatusCodes.Status201Created, response);
    }
}
