using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Forma.Domain.Builders.Contracts;
using Forma.Domain.Builders;

namespace Forma.Domain;

[ExcludeFromCodeCoverage]
public static class ConfigureServices
{
    /// <summary>
    /// Adds Entities Builders the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddEntitiesBuilders(this IServiceCollection services) =>
        services.AddScoped<IExerciseBuilder, ExerciseBuilder>();
    
}