using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeKeep.Models;
using TimeKeep.RPC.Locations;

namespace TimeKeep.Features.Locations;

public class LocationsService
{
	private readonly TimeKeepContext context;

	public LocationsService(TimeKeepContext context) =>
		this.context = context;

	public IQueryable<ResponseLocation> List(ListRequest request)
	{
		IQueryable<Models.Location> query = context.Locations.AsNoTracking();

		query = request.Order switch
		{
			Order.UsageAsc => query.OrderBy(location => location.Entries.Max(entry => entry.Start)),
			Order.UsageDesc => query.OrderByDescending(location => location.Entries.Max(entry => entry.Start)),
			Order.NameAsc => query.OrderBy(location => location.Name),
			Order.NameDesc => query.OrderByDescending(location => location.Name),
		};

		return query
			.Select(location => new ResponseLocation(
				location.Id,
				location.Name,
				location.Address));
	}

	public async Task<CreateResponse> Create(CreateRequest request)
	{
		var location = new Models.Location
		{
			Address = request.Address,
		};
		if (request.HasName)
		{
			location.Name = request.Name;
		}
		await context.Locations.AddAsync(location);
		await context.SaveChangesAsync();
		return new CreateResponse { Status = CreateStatus.Success };
	}

	public async Task<SetNameResponse> SetName(SetNameRequest request)
	{
		var location = request.IdentifierCase switch
		{
			SetNameRequest.IdentifierOneofCase.None => null,
			SetNameRequest.IdentifierOneofCase.Id => await context.Locations.FindAsync(request.Id),
			SetNameRequest.IdentifierOneofCase.OldName => await context.Locations.FirstAsync(l => l.Name == request.OldName),
		};
		if (location is null)
		{
			return new SetNameResponse
			{
				Status = SetNameStatus.LocationNotFound,
			};
		}
		location.Name = request.Name;
		await context.SaveChangesAsync();
		return new SetNameResponse
		{
			Status = SetNameStatus.Success,
		};
	}

	public async Task<SetAddressResponse> SetAddress(SetAddressRequest request)
	{
		var location = request.IdentifierCase switch
		{
			SetAddressRequest.IdentifierOneofCase.None => null,
			SetAddressRequest.IdentifierOneofCase.Id => await context.Locations.FindAsync(request.Id),
			SetAddressRequest.IdentifierOneofCase.Name => await context.Locations.FirstAsync(l => l.Name == request.Name),
		};
		if (location is null)
		{
			return new SetAddressResponse
			{
				Status = SetAddressStatus.LocationNotFound,
			};
		}
		location.Address = request.Address;
		await context.SaveChangesAsync();
		return new SetAddressResponse
		{
			Status = SetAddressStatus.Success,
		};
	}

	public async Task<DestroyResponse> Destroy(DestroyRequest request)
	{
		var location = request.IdentifierCase switch
		{
			DestroyRequest.IdentifierOneofCase.None => null,
			DestroyRequest.IdentifierOneofCase.Id => await context.Locations.FindAsync(Guid.Parse(request.Id)),
			DestroyRequest.IdentifierOneofCase.Name => await context.Locations.FirstAsync(l => l.Name == request.Name),
		};
		if (location is null)
		{
			return new DestroyResponse { Status = DestroyStatus.LocationNotFound };
		}
		context.Locations.Remove(location);
		await context.SaveChangesAsync();
		return new DestroyResponse { Status = DestroyStatus.Success };
	}
}

public record ResponseLocation(Guid Id, string? Name, string Address);
