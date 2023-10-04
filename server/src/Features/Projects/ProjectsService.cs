using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeKeep.Models;
using TimeKeep.RPC.Projects;
using RPCProject = TimeKeep.RPC.Projects.Project;
using Microsoft.EntityFrameworkCore;

namespace TimeKeep.Features.Projects;

public class ProjectsService
{
	private readonly TimeKeepContext context;

	public ProjectsService(TimeKeepContext context) =>
		this.context = context;

	public IQueryable<RPCProject> List(ListRequest request)
	{
		IQueryable<Models.Project> query = context.Projects.AsNoTracking();

		query = request.Order switch
		{
			Order.UsageAsc => query.OrderBy(project => project.Categories.Min(c => c.Entries.Max(e => e.Start))),
			Order.UsageDesc => query.OrderByDescending(project => project.Categories.Min(c => c.Entries.Max(e => e.Start))),
			Order.AlphaAsc => query.OrderBy(project => project.Name),
			Order.AlphaDesc => query.OrderByDescending(project => project.Name),
		};

		return query
			.Select(project => new RPCProject { Name = project.Name });
	}

	public async Task<CreateResponse> Create(CreateRequest request)
	{
		var project = new Models.Project
		{
			Name = request.Name,
			Categories = new List<Category>(request.Categories.Count),
		};

		var categories = context.Categories
			.Where(c => request.Categories.Contains(c.Name))
			.AsAsyncEnumerable();
		await foreach (var category in categories)
		{
			project.Categories.Add(category);
		}

		await context.Projects.AddAsync(project);
		await context.SaveChangesAsync();
		return new CreateResponse { Status = CreateStatus.Success };
	}

	public async Task<AddCategoriesResponse> AddCategories(AddCategoriesRequest request)
	{
		var project = await context.Projects.FindAsync(request.Name);
		if (project is null)
		{
			return new AddCategoriesResponse { Status = AddCategoriesStatus.ProjectNotFound };
		}
		await context.Entry(project).Collection(p => p.Categories).LoadAsync();

		var categories = context.Categories
			.Where(c => request.Categories.Contains(c.Name))
			.AsAsyncEnumerable();
		await foreach (var category in categories)
		{
			project.Categories.Add(category);
		}

		await context.SaveChangesAsync();
		return new AddCategoriesResponse { Status = AddCategoriesStatus.Success };
	}

	public async Task<RemoveCategoriesResponse> RemoveCategories(RemoveCategoriesRequest request)
	{
		var project = await context.Projects.FindAsync(request.Name);
		if (project is null)
		{
			return new RemoveCategoriesResponse { Status = RemoveCategoriesStatus.ProjectNotFound };
		}
		await context.Entry(project).Collection(p => p.Categories).LoadAsync();

		var categories = context.Categories
			.Where(c => request.Categories.Contains(c.Name))
			.AsAsyncEnumerable();
		await foreach (var category in categories)
		{
			project.Categories.Remove(category);
		}

		await context.SaveChangesAsync();
		return new RemoveCategoriesResponse { Status = RemoveCategoriesStatus.Success };
	}

	public async Task<DestroyResponse> Destroy(DestroyRequest request)
	{
		var project = await context.Projects.FindAsync(request.Name);
		if (project is null)
		{
			return new DestroyResponse { Status = DestroyStatus.ProjectNotFound };
		}
		context.Projects.Remove(project);
		await context.SaveChangesAsync();
		return new DestroyResponse { Status = DestroyStatus.Success };
	}
}
