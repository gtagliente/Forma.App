using System;
using System.Diagnostics.CodeAnalysis;
using Forma.CoreContext.SharedKernel;
using Forma.CoreInfrastructure.Abstractions;
using Forma.Domain.Entities.ExerciseAggregate;
using Forma.Domain.Entities.ExerciseAggregate.Contracts;

// using Forma.Domain.Entities.CustomerAggregate;
using Forma.Infrastructure.Data;
using Forma.Infrastructure.Data.Context;
using Forma.Infrastructure.Data.Repositories;
using Forma.Infrastructure.Data.Services;
using System.Net.Http;
using Forma.Infrastructure.ExternalServices.TrainingPlanning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Polly.Timeout;
using Polly;

namespace Forma.Infrastructure;

[ExcludeFromCodeCoverage]
public static class ConfigureServices
{
    /// <summary>
    /// Adds the memory cache service to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void AddMemoryCacheService(this IServiceCollection services) =>
        services.AddScoped<ICacheService, MemoryCacheService>();

    /// <summary>
    /// Adds the distributed cache service to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void AddDistributedCacheService(this IServiceCollection services) =>
        services.AddScoped<ICacheService, DistributedCacheService>();

    /// <summary>
    /// Adds the infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services) =>
        services
            .AddScoped<WriteDbContext>()
            .AddScoped<EventStoreDbContext>()
            .AddScoped<IUnitOfWork, UnitOfWork>();

    /// <summary>
    /// Adds the write-only repositories to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddWriteOnlyRepositories(this IServiceCollection services) =>
         services
            .AddScoped<IEventStoreRepository<EventStore>, EventStoreRepository>()
            .AddScoped<IExerciseWriteOnlyRepository<Exercise, ExerciseId>, ExerciseWriteOnlyRepository>()
            .AddScoped<IExerciseResourceWriteOnlyRepository<ExerciseResource, ExerciseResourceId>, ExerciseResourceWriteOnlyRepository>()
            .AddScoped<IExerciseUniquenessChecker, ExerciseWriteOnlyRepository>()
            .AddScoped<IExerciseResourceLinkUniquenessChecker, ExerciseWriteOnlyRepository>()
            .AddScoped<IExerciseHierarchyChecker, ExerciseWriteOnlyRepository>();

    /// <summary>
    /// Adds typed HTTP clients for the other services this one calls directly (ADR-006 —
    /// Forma.Claude/docs/architecture/adr/ADR-006-cross-service-reference-integrity.md).
    /// Unauthenticated for now — a deliberate, tracked deferral until identity-service exists
    /// (see Forma.Claude/docs/architecture/integration-patterns.md, "Security note").
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">App configuration, for the callee's base URL.</param>
    public static IServiceCollection AddExternalServiceClients(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHttpClient("TrainingPlanningService", client =>
            {
                var baseUrl = configuration["Services:TrainingPlanningService:BaseUrl"]
                    ?? throw new InvalidOperationException("Missing configuration: Services:TrainingPlanningService:BaseUrl");
                client.BaseAddress = new Uri(baseUrl);
            })
            // ADR-006 Rule 2 (Exercise delete -> Workout reference check): fails closed, so a
            // longer timeout is acceptable here — a slower delete is a much smaller cost than a
            // silently orphaned reference. See integration-patterns.md's per-direction table.
            .AddResilienceHandler("training-planning-service", builder =>
                builder.AddTimeout(TimeSpan.FromSeconds(5)));

        return services
            .AddScoped<IRequestAdapter>(provider =>
            {
                var httpClient = provider
                    .GetRequiredService<IHttpClientFactory>()
                    .CreateClient("TrainingPlanningService");
                return new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
            })
            .AddScoped<TrainingPlanningApiClient>()
            .AddScoped<IExerciseUsageChecker, ExerciseUsageChecker>();
    }
}
