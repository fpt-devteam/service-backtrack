
using Backtrack.Core.WebApi.Contracts.Common;
using Backtrack.Core.WebApi.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

public static class ControllerExtensions
{
    public static IActionResult ApiOk<T>(this ControllerBase controller, T data)
    {
        var response = ApiResponse<T>.SuccessResponse(data, HttpContextUtil.GetCorrelationId(controller.HttpContext));
        return controller.Ok(response);
    }

    public static IActionResult ApiCreated<T>(this ControllerBase controller, T data)
    {
        var response = ApiResponse<T>.SuccessResponse(data, HttpContextUtil.GetCorrelationId(controller.HttpContext));
        return controller.StatusCode(StatusCodes.Status201Created, response);
    }
}
