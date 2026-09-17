using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using TimeKeep.Models;

namespace TimeKeep.Api;

public static class WebAppApi
{
    public static IEndpointConventionBuilder MapWebAppApi(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api");
        group.MapGet("/init", GetInit);
        group.MapPost("/entry", CreateEntry);
        group.MapPatch("/entry/{id:guid}", UpdateEntry);
        group.MapDelete("/entry/{id:guid}", DeleteEntry);
        group.MapPost("/project", CreateProject);
        group.MapPost("/category", CreateCategory);
        return group;
    }

    private static async Task<Ok<Init>> GetInit([FromServices] TimeKeepContext context, CancellationToken cancellationToken)
    {
        var categories = await context.Categories.AsNoTracking()
            .OrderByDescending(category => category.Projects.Count())
            .Select(c => c.Name)
            .ToArrayAsync(cancellationToken);
        var today = DateTime.UtcNow.Date;
        var entries = await context.Entries.AsNoTracking()
            .Where(e => today < e.Start || e.End == null || today < e.End)
            .Select(e => new Entry(
                e.Id,
                e.Start,
                e.End,
                e.Location != null
                    ? new(e.Location.Id, e.Location.Name, e.Location.Address)
                    : null,
                e.Categories.Select(c => c.Name).ToArray()
            ))
            .ToArrayAsync(cancellationToken);
        var locations = await context.Locations.AsNoTracking()
            .OrderByDescending(location => location.Entries.Max(entry => entry.Start))
            .Select(l => new Location(l.Id, l.Name, l.Address))
            .ToArrayAsync(cancellationToken);
        var projects = await context.Projects.AsNoTracking()
            .OrderByDescending(p => p.Categories.Min(c => c.Entries.Max(e => e.Start)))
            .Select(p => p.Name)
            .ToArrayAsync(cancellationToken);
        return TypedResults.Ok(new Init(categories, entries, locations, projects));
    }

    private static async Task<Ok<Entry>> CreateEntry([FromServices] TimeKeepContext context, [FromBody] EntryRequest request, CancellationToken cancellationToken)
    {
        var entry = new Models.Entry
        {
            Id = Guid.NewGuid(),
            Start = request.Start,
            End = request.End,
            Location = request.Location is { } locationId
                ? await context.Locations.FindAsync([locationId], cancellationToken)
                : null,
            Categories = await context.Categories
                .Where(c => request.Categories.Contains(c.Name) || c.Projects.Any(p => p.Name == request.Project))
                .ToArrayAsync(cancellationToken),
        };
        context.Entries.Add(entry);
        await context.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(new Entry(
            entry.Id,
            entry.Start,
            entry.End,
            entry.Location != null
                ? new(entry.Location.Id, entry.Location.Name, entry.Location.Address)
                : null,
            entry.Categories.Select(c => c.Name).ToArray()
        ));
    }

    private static async Task<Results<Ok<Entry>, NotFound>> UpdateEntry([FromServices] TimeKeepContext context, [FromRoute] Guid id, [FromBody] EntryRequest request, CancellationToken cancellationToken)
    {
        var entry = await context.Entries.FindAsync([id], cancellationToken);
        if (entry is null)
        {
            return TypedResults.NotFound();
        }
        await context.Entry(entry).Collection(e => e.Categories).LoadAsync(cancellationToken);

        if (entry.Start != request.Start)
        {
            entry.Start = request.Start;
        }
        
        if (entry.End != request.End)
        {
            entry.End = request.End;
        }

        if (entry.Location?.Id != request.Location)
        {
            if (request.Location is null)
            {
                entry.Location = null;
            }
            else
            {
                entry.Location = await context.Locations.FindAsync([request.Location], cancellationToken);
            }
        }

        foreach (var category in entry.Categories.ToArray())
        {
            if (!request.Categories.Any(c => c == category.Name))
            {
                entry.Categories.Remove(category);
            }
        }
        if (request.Project is { } projectName)
        {
            var projectCategories = await context.Projects
                .Where(p => p.Name == projectName).SelectMany(p => p.Categories)
                .ToArrayAsync(cancellationToken);
            foreach (var category in projectCategories)
            {
                if (!entry.Categories.Any(c => c.Name == category.Name))
                {
                    entry.Categories.Add(category);
                }
            }
        }
        var missingCategoryNames = request.Categories.Where(name => !entry.Categories.Any(c => c.Name == name)).ToArray();
        if (missingCategoryNames.Length > 0)
        {
            var missingCategories = await context.Categories
                .Where(c => missingCategoryNames.Contains(c.Name))
                .ToArrayAsync(cancellationToken);
            foreach (var missingCategory in missingCategories)
            {
                entry.Categories.Add(missingCategory);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(new Entry(
            entry.Id,
            entry.Start,
            entry.End,
            entry.Location != null
                ? new(entry.Location.Id, entry.Location.Name, entry.Location.Address)
                : null,
            entry.Categories.Select(c => c.Name).ToArray()
        ));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteEntry([FromServices] TimeKeepContext context, [FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var entry = await context.Entries.FindAsync([id], cancellationToken);
        if (entry is null)
        {
            return TypedResults.NotFound();
        }
        context.Entries.Remove(entry);
        await context.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> CreateCategory([FromServices] TimeKeepContext context, [FromBody] CategoryRequest request, CancellationToken cancellationToken)
    {
        var existingCategory = await context.Categories.FindAsync([request.Category], cancellationToken);
        if (existingCategory is not null)
        {
            return TypedResults.NoContent();
        }
        var category = new Category { Name = request.Category };
        context.Categories.Add(category);
        await context.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, Conflict>> CreateProject([FromServices] TimeKeepContext context, [FromBody] ProjectRequest request, CancellationToken cancellationToken)
    {
        var existingProject = await context.Projects.FindAsync([request.Project], cancellationToken);
        if (existingProject is not null)
        {
            return TypedResults.Conflict();
        }
        var project = new Project
        {
            Name = request.Project,
            Categories = await context.Categories.Where(c => request.Categories.Contains(c.Name)).ToArrayAsync(cancellationToken),
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }
}

public sealed record Init(
    [property: JsonPropertyName("categories")] string[] Categories,
    [property: JsonPropertyName("entries")] Entry[] Entries,
    [property: JsonPropertyName("locations")] Location[] Locations,
    [property: JsonPropertyName("projects")] string[] Projects
);

public sealed record Entry(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("start")] DateTime Start,
    [property: JsonPropertyName("end")] DateTime? End,
    [property: JsonPropertyName("location")] Location? Location,
    [property: JsonPropertyName("categories")] string[] Categories
);

public sealed record Location(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("address")] string? Address
);

public sealed record EntryRequest(
    [property: JsonPropertyName("start")] DateTime Start,
    [property: JsonPropertyName("end")] DateTime? End,
    [property: JsonPropertyName("project")] string? Project,
    [property: JsonPropertyName("location")] Guid? Location,
    [property: JsonPropertyName("categories")] string[] Categories
);

public sealed record CategoryRequest([property: JsonPropertyName("category")] string Category);

public sealed record ProjectRequest(
    [property: JsonPropertyName("project")] string Project,
    [property: JsonPropertyName("categories")] string[] Categories
);
