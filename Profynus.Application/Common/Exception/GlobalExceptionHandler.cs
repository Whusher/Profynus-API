namespace Profynus.Application.Common.Exception;

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;


public class UnauthorizedException(string message) : Exception(message);
public class ConflictException(string message)     : Exception(message);
public class NotFoundException(string message)     : Exception(message);

// ── Global exception handler (ASP.NET Core 8 IExceptionHandler) ──────────────

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, title) = ex switch
        {
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ConflictException     => (StatusCodes.Status409Conflict,     "Conflict"),
            NotFoundException     => (StatusCodes.Status404NotFound,     "Not found"),
            _                     => (StatusCodes.Status500InternalServerError, "Internal error"),
        };

        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title  = title,
            Detail = ex.Message,
        }, ct);

        return true;
    }
}