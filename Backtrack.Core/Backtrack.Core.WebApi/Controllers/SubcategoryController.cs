using Backtrack.Core.Application.Usecases.Subcategories;
using Backtrack.Core.Application.Usecases.Subcategories.GetSubcategories;
using Backtrack.Core.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backtrack.Core.WebApi.Controllers;

[ApiController]
[Route("subcategories")]
[Produces("application/json")]
public sealed class SubcategoryController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SubcategoryResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubcategoriesAsync(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetSubcategoriesQuery(), cancellationToken);
        return this.ApiOk(result);
    }
}
