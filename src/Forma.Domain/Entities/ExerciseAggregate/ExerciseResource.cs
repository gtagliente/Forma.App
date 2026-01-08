using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forma.CoreContext.SharedKernel.Exceptions.DomainExceptions;
using Forma.Domain.Builders.Contracts;
using Forma.Domain.Entities.ExerciseAggregate.ValueObjects;

namespace Forma.Domain.Entities.ExerciseAggregate;

public class ExerciseResource
{
    public ExerciseResourceId ExerciseResourceId { get; private set; }

    public ExerciseId ExerciseId { get; private set; }

    public string Title { get; private set; }

    public string Content { get; private set; }

    public ResourceType Type { get; private set; }

    public string Link { get; private set; }

    private ExerciseResource()
    {
    }

    private ExerciseResource(ExerciseResourceId exerciseResourceId, ExerciseId exerciseId, string title, string content, ResourceType type, string link)
    {
        ExerciseResourceId = exerciseResourceId;
        ExerciseId = exerciseId;
        Title = title;
        Content = content;
        Type = type;
        Link = link;

        //var x = ExerciseResource.Create(null, exerciseId, title, content, type, link);
    }

    internal static ExerciseResource Create(IExerciseBuilder exerciseBuilder,ExerciseId exerciseId, string title, string content, ResourceType type, string link)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainArgumentException("Title is required.");
        if (string.IsNullOrWhiteSpace(link))
            throw new DomainArgumentException("Link is required.");
        //Check valid link
        return new ExerciseResource(
            ExerciseResourceId.New(),
            exerciseId,
            title,
            content,
            type,
            link
        );
    }
}


public readonly record struct ExerciseResourceId(Guid Value)
{
    public static ExerciseResourceId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
