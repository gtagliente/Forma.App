using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Forma.Infrastructure.Data.Context;
using Forma.PublicApi.Models;
using Forma.Query.Abstractions;
using Forma.Query.Data.Context;
using Xunit;
using Xunit.Categories;
using Forma.Application.Exercise.Commands;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;
using Forma.IntegrationTests.Extensions;
using Forma.Application.Exercise.Responses;
using Forma.CoreInfrastructure.Extensions;
using Forma.IntegrationTests.Infrastructure;

namespace Shop.IntegrationTests.Controllers.V1;

[IntegrationTest]
public class ExercisesControllerTests : BaseIntegrationTest
{
    //private const string ConnectionString = "Data Source=:memory:";
    private const string Endpoint = "/api/exercises";
    //private readonly SqliteConnection _eventStoreDbContextSqlite = new(ConnectionString);
    //private readonly SqliteConnection _writeDbContextSqlite = new(ConnectionString);

    public ExercisesControllerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    #region POST: /api/exercises/

    [Fact]
    public async Task Should_ReturnsHttpStatus201Created_When_Post_ValidRequest()
    {
        //await WriteDbContextExecuteRawSql(@"INSERT INTO Exercise (Id,Name,Description,MuscleGroups) VALUES (N'222637A6-CF25-41FB-9432-993622722DD2',N'Pull Up',N'Descr',N'3|1');");
        // Arrange
        //await using var webApplicationFactory = InitializeWebAppFactory();
        using var httpClient = factory.CreateClient(CreateClientOptions());

        var command = new Faker<CreateExerciseCommand>()
            .RuleFor(command => command.Name, "Pull Up")
            .RuleFor(command => command.Description, faker => faker.Lorem.Sentence(10))
            .RuleFor(command => command.MuscleGroups, faker => new List<MuscleGroup>(){ MuscleGroup.Shoulders, MuscleGroup.Back}.ToArray() )
            .Generate();

        // Act
        using var jsonContent = command.ToJsonHttpContent();
        using var act = await httpClient.PostAsync(Endpoint, jsonContent);

        // Assert (HTTP)
        act.Should().NotBeNull();
        act.IsSuccessStatusCode.Should().BeTrue();
        act.StatusCode.Should().Be(HttpStatusCode.Created);

        // Assert (HTTP Content Response)
        var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<CreatedExerciseResponse>>();
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.StatusCode.Should().Be(StatusCodes.Status201Created);
        response.Errors.Should().BeEmpty();
        response.Result.Should().NotBeNull();
        response.Result.Id.Should().NotBeEmpty();

        // Assert Location Header
        act.Headers.GetValues("Location").Should().NotBeNullOrEmpty()
            .And.Contain($"/api/exercises/{response.Result.Id}");
    }

    //[Fact]
    //public async Task Should_ReturnsHttpStatus400_When_Post_ExerciseNameIsNotUnique()
    //{
    //    await WriteDbContextExecuteRawSql(@"
    //        DELETE FROM Exercise;
    //        INSERT INTO Exercise (Id,Name,Description,MuscleGroups) VALUES (N'222637A6-CF25-41FB-9432-993622722DD2',N'Pull Up',N'Descr',N'3|1');
    //    ");
    //    // Arrange
    //    //await using var webApplicationFactory = InitializeWebAppFactory();
    //    using var httpClient = factory.CreateClient(CreateClientOptions());

    //    var command = new Faker<CreateExerciseCommand>()
    //        .RuleFor(command => command.Name, "Pull Up")
    //        .RuleFor(command => command.Description, faker => faker.Lorem.Sentence(10))
    //        .RuleFor(command => command.MuscleGroups, faker => new List<MuscleGroup>() { MuscleGroup.Shoulders, MuscleGroup.Back }.ToArray())
    //        .Generate();

    //    // Act
    //    using var jsonContent = command.ToJsonHttpContent();
    //    using var act = await httpClient.PostAsync(Endpoint, jsonContent);

    //    // Assert (HTTP)
    //    act.Should().NotBeNull();
    //    act.IsSuccessStatusCode.Should().BeFalse();
    //    act.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    //    // Assert (HTTP Content Response)
    //    var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<CreatedExerciseResponse>>();
    //    response.Should().NotBeNull();
    //    response.Success.Should().BeFalse();
    //    response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    //    response.Errors.Should().NotBeNullOrEmpty();
    //    response.Result.Should().NotBeNull();
    //    response.Result.Id.Should().NotBeEmpty();

    //    // Assert Location Header
    //    act.Headers.GetValues("Location").Should().NotBeNullOrEmpty()
    //        .And.Contain($"/api/exercises/{response.Result.Id}");
    //}

    #endregion
    #region Helpers
    private static WebApplicationFactoryClientOptions CreateClientOptions() => new() { AllowAutoRedirect = false };

    #endregion
}