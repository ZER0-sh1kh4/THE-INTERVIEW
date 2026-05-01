using BuildingBlocks.Messaging;
using BuildingBlocks.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Extensions;

/// <summary>
/// Shared dependency registration helpers used by all services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers RabbitMQ settings and the shared publisher.
    /// </summary>
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        return services;
    }

    /// <summary>
    /// Configures shared API behavior, including validation responses.
    /// </summary>
    public static IServiceCollection AddApiDefaults(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = string.Join("; ", context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid request." : e.ErrorMessage));

                return new BadRequestObjectResult(new ApiErrorResponse
                {
                    Message = "Validation failed.",
                    Details = errors
                });
            };
        });

        return services;
    }
}
