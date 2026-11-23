using HarmonyPlugins.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace HarmonyPlugins.Api.Middlewares;

public class AppExceptionHandler(IProblemDetailsService problemDetails, ILogger<AppExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, detail) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, exception.Message),
            BusinessRuleException => (StatusCodes.Status400BadRequest, exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, exception.Message),
            // Erros inesperados não expõem detalhes internos ao cliente.
            _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado. Tente novamente.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Erro não tratado em {Path}", context.Request.Path);

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = { Status = status, Detail = detail },
            Exception = exception
        });
    }
}
