using BuildingBlocks.Middleware;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Extensions;

/// <summary>
/// Shared application builder helpers.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the shared global exception middleware.
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
