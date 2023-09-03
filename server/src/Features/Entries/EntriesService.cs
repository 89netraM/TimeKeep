using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using TimeKeep.Models;
using TimeKeep.RPC.Entries;
using RpcEntry = TimeKeep.RPC.Entries.Entry;

namespace TimeKeep.Features.Entries;

public class EntriesService
{
	private readonly TimeKeepContext context;

	public EntriesService(TimeKeepContext context) =>
		this.context = context;

	public IQueryable<ResponseEntry> List(ListRequest listRequest)
	{
		IQueryable<Models.Entry> query = context.Entries;

		if (listRequest.EndStatus is EndStatus.OnlyCompleted)
		{
			query = query.Where(entry => entry.End != null);
		}
		else if (listRequest.EndStatus is EndStatus.OnlyActive)
		{
			query = query.Where(entry => entry.End == null);
		}

		if (listRequest.Order is Order.StartAsc)
		{
			query = query.OrderBy(entry => entry.Start);
		}
		else if (listRequest.Order is Order.StartDesc)
		{
			query = query.OrderByDescending(entry => entry.Start);
		}
		else if (listRequest.Order is Order.EndAsc)
		{
			query = query.OrderBy(entry => entry.End);
		}
		else if (listRequest.Order is Order.EndDesc)
		{
			query = query.OrderByDescending(entry => entry.End);
		}

		var mustCategories = new List<string>();
		var mustNotCategories = new List<string>();
		foreach (var category in listRequest.Categories)
		{
			if (category[0] == '!')
			{
				mustNotCategories.Add(category[1..]);
			}
			else
			{
				mustCategories.Add(category);
			}
		}
		if (mustCategories.Count > 0)
		{
			query = query.Where(entry => entry.Categories.Count(c => mustCategories.Contains(c.Name)) == mustCategories.Count);
		}
		if (mustNotCategories.Count > 0)
		{
			query = query.Where(entry => entry.Categories.Count(c => mustNotCategories.Contains(c.Name)) == 0);
		}

		if (listRequest.After is not null)
		{
			var after = listRequest.After.ToDateTime();
			query = query.Where(entry => after < entry.Start);
		}
		if (listRequest.Before is not null)
		{
			var before = listRequest.Before.ToDateTime();
			query = query.Where(entry => entry.Start < before);
		}

		return query
			.Select(entry => new ResponseEntry(
				entry.Id,
				entry.Start,
				entry.End,
				entry.Categories.Select(category => category.Name).ToList(),
				entry.Location != null
					? new(entry.Location.Id, entry.Location.Name, entry.Location.Address)
					: null));
	}

	public async Task<GetResponse> Get(GetRequest getRequest)
	{
		if (!Guid.TryParse(getRequest.Id, out Guid id))
		{
			return new GetResponse { Status = GetStatus.EntryNotFound };
		}
		var entry = await context.Entries.FindAsync(id);
		if (entry is null)
		{
			return new GetResponse { Status = GetStatus.EntryNotFound };
		}
		await context.Entry(entry).Collection(e => e.Categories).LoadAsync();

		var rpcEntry = new RpcEntry
		{
			Id = entry.Id.ToString(),
			Start = Timestamp.FromDateTime(entry.Start),
			End = entry.End is DateTime end ? Timestamp.FromDateTime(end) : null,
		};
		rpcEntry.Categories.AddRange(entry.Categories.Select(c => c.Name));
		return new GetResponse
		{
			Status = GetStatus.Success,
			Entry = rpcEntry,
		};
	}

	public async Task<CreateResponse> Create(CreateRequest createRequest)
	{
		var categories = new List<Category>();
		if (createRequest.HasProject)
		{
			var project = await context.Projects.FindAsync(createRequest.Project);
			if (project is null)
			{
				return new CreateResponse { Status = CreateStatus.ProjectNotFound };
			}
			await context.Entry(project).Collection(p => p.Categories).LoadAsync();
			categories.Capacity += project.Categories.Count;
			foreach (var category in project.Categories)
			{
				categories.Add(category);
			}
		}
		if (createRequest.Categories is ICollection<string> categoryNames)
		{
			categories.Capacity += categoryNames.Count;
			foreach (var categoryName in categoryNames)
			{
				var category = await context.Categories.FindAsync(categoryName);
				if (category is null)
				{
					return new CreateResponse { Status = CreateStatus.CategoryNotFound };
				}
				categories.Add(category);
			}
		}

		Location? location = null;
		if (createRequest.LocationIdentifierCase != CreateRequest.LocationIdentifierOneofCase.None)
		{
			location = createRequest.LocationIdentifierCase switch
			{
				CreateRequest.LocationIdentifierOneofCase.None => throw new InvalidOperationException(),
				CreateRequest.LocationIdentifierOneofCase.LocationId => await context.Locations.FindAsync(Guid.Parse(createRequest.LocationId)),
				CreateRequest.LocationIdentifierOneofCase.LocationName => await context.Locations.FirstAsync(l => l.Name == createRequest.LocationName),
			};
			if (location is null)
			{
				return new CreateResponse { Status = CreateStatus.LocationNotFound };
			}
		}

		var entry = new Models.Entry
		{
			Start = createRequest.Start.ToDateTime(),
			End = createRequest.End?.ToDateTime(),
			Categories = categories,
			Location = location,
		};
		await context.Entries.AddAsync(entry);
		await context.SaveChangesAsync();
		return new CreateResponse
		{
			Status = CreateStatus.Success,
			Id = entry.Id.ToString(),
		};
	}

	public async Task<SetStartResponse> SetStart(SetStartRequest setStartRequest)
	{
		if (!Guid.TryParse(setStartRequest.Id, out Guid id))
		{
			return new SetStartResponse { Status = SetStartStatus.EntryNotFound };
		}
		var entry = await context.Entries.FindAsync(id);
		if (entry is null)
		{
			return new SetStartResponse { Status = SetStartStatus.EntryNotFound };
		}

		entry.Start = setStartRequest.Start.ToDateTime();

		await context.SaveChangesAsync();
		return new SetStartResponse { Status = SetStartStatus.Success };
	}

	public async Task<SetEndResponse> SetEnd(SetEndRequest setEndRequest)
	{
		if (!Guid.TryParse(setEndRequest.Id, out Guid id))
		{
			return new SetEndResponse { Status = SetEndStatus.EntryNotFound };
		}
		var entry = await context.Entries.FindAsync(id);
		if (entry is null)
		{
			return new SetEndResponse { Status = SetEndStatus.EntryNotFound };
		}

		entry.End = setEndRequest.End.ToDateTime();

		await context.SaveChangesAsync();
		return new SetEndResponse { Status = SetEndStatus.Success };
	}

	public async Task<AddCategoriesResponse> AddCategories(AddCategoriesRequest addCategoriesRequest)
	{
		if (!Guid.TryParse(addCategoriesRequest.Id, out Guid id))
		{
			return new AddCategoriesResponse { Status = AddCategoriesStatus.EntryNotFound };
		}
		var entry = await context.Entries.FindAsync(id);
		if (entry is null)
		{
			return new AddCategoriesResponse { Status = AddCategoriesStatus.EntryNotFound };
		}
		await context.Entry(entry).Collection(e => e.Categories).LoadAsync();

		if (addCategoriesRequest.Categories is ICollection<string> categoryNames)
		{
			var categories = context.Categories
				.Where(c => categoryNames.Contains(c.Name))
				.AsAsyncEnumerable();
			await foreach (var category in categories)
			{
				entry.Categories.Add(category);
			}
		}

		await context.SaveChangesAsync();
		return new AddCategoriesResponse { Status = AddCategoriesStatus.Success };
	}

	public async Task<RemoveCategoriesResponse> RemoveCategories(RemoveCategoriesRequest removeCategoriesRequest)
	{
		if (!Guid.TryParse(removeCategoriesRequest.Id, out Guid id))
		{
			return new RemoveCategoriesResponse { Status = RemoveCategoriesStatus.EntryNotFound };
		}
		var entry = await context.Entries.FindAsync(id);
		if (entry is null)
		{
			return new RemoveCategoriesResponse { Status = RemoveCategoriesStatus.EntryNotFound };
		}
		await context.Entry(entry).Collection(e => e.Categories).LoadAsync();

		if (removeCategoriesRequest.Categories is ICollection<string> categoryNames)
		{
			var categories = context.Categories
				.Where(c => categoryNames.Contains(c.Name))
				.AsAsyncEnumerable();
			await foreach (var category in categories)
			{
				entry.Categories.Remove(category);
			}
		}

		await context.SaveChangesAsync();
		return new RemoveCategoriesResponse { Status = RemoveCategoriesStatus.Success };
	}

	public async Task<SetLocationResponse> SetLocation(SetLocationRequest setLocationRequest)
	{
		if (!Guid.TryParse(setLocationRequest.Id, out Guid id))
		{
			return new SetLocationResponse { Status = SetLocationStatus.EntryNotFound };
		}
		var entry = await context.Entries.FindAsync(id);
		if (entry is null)
		{
			return new SetLocationResponse { Status = SetLocationStatus.EntryNotFound };
		}

		if (setLocationRequest.LocationIdentifierCase == SetLocationRequest.LocationIdentifierOneofCase.None)
		{
			entry.Location = null;
		}
		else
		{
			var location = setLocationRequest.LocationIdentifierCase switch
			{
				SetLocationRequest.LocationIdentifierOneofCase.None => throw new InvalidOperationException(),
				SetLocationRequest.LocationIdentifierOneofCase.LocationId => await context.Locations.FindAsync(Guid.Parse(setLocationRequest.LocationId)),
				SetLocationRequest.LocationIdentifierOneofCase.LocationName => await context.Locations.FirstAsync(l => l.Name == setLocationRequest.LocationName),
			};
			if (location is null)
			{
				return new SetLocationResponse { Status = SetLocationStatus.LocationNotFound };
			}
			entry.Location = location;
		}

		await context.SaveChangesAsync();

		return new SetLocationResponse { Status = SetLocationStatus.Success };
	}

	public async Task<DestroyResponse> Destroy(DestroyRequest removeRequest)
	{
		if (!Guid.TryParse(removeRequest.Id, out Guid id))
		{
			return new DestroyResponse { Status = DestroyStatus.EntryNotFound };
		}
		var entry = await context.Entries.FindAsync(id);
		if (entry is null)
		{
			return new DestroyResponse { Status = DestroyStatus.EntryNotFound };
		}
		context.Entries.Remove(entry);
		await context.SaveChangesAsync();
		return new DestroyResponse { Status = DestroyStatus.Success };
	}
}

public record ResponseEntry(
	Guid Id,
	DateTime Start,
	DateTime? End,
	ICollection<string> Categories,
	ResponseLocation? Location);

public record ResponseLocation(
	Guid Id,
	string? Name,
	string Address);
