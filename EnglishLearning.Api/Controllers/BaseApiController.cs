using EnglishLearning.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.Error.Type switch
        {
            ErrorType.NotFound => NotFound(new { error = new { code = result.Error.Code, message = result.Error.Message } }),
            ErrorType.Validation => BadRequest(new { error = new { code = result.Error.Code, message = result.Error.Message } }),
            ErrorType.Conflict => Conflict(new { error = new { code = result.Error.Code, message = result.Error.Message } }),
            ErrorType.Unauthorized => Unauthorized(new { error = new { code = result.Error.Code, message = result.Error.Message } }),
            ErrorType.Forbidden => StatusCode(403, new { error = new { code = result.Error.Code, message = result.Error.Message } }),
            _ => StatusCode(500, new { error = new { code = result.Error.Code, message = result.Error.Message } })
        };
    }
}