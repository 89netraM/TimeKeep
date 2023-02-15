using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TimeKeep.RPC.Locations;
using static TimeKeep.RPC.Locations.LocationsService;

namespace TimeKeep.Features.Locations;

[Authorize]
public class LocationsRPCService : LocationsServiceBase
{
	private readonly LocationsService service;

	public LocationsRPCService(LocationsService service) =>
		this.service = service;

	public override async Task List(ListRequest request, IServerStreamWriter<Location> responseStream, ServerCallContext context)
	{
		await foreach (var location in service.List(request).AsAsyncEnumerable())
		{
			if (context.CancellationToken.IsCancellationRequested)
			{
				break;
			}
			var rpcLocation = new Location
			{
				Id = location.Id.ToString(),
				Address = location.Address,
			};
			if (location.Name is string name)
			{
				rpcLocation.Name = location.Name;
			}
			await responseStream.WriteAsync(rpcLocation);
			if (context.CancellationToken.IsCancellationRequested)
			{
				break;
			}
		}
	}

	public override Task<CreateResponse> Create(CreateRequest request, ServerCallContext context) =>
		service.Create(request);

	public override Task<SetNameResponse> SetName(SetNameRequest request, ServerCallContext context) =>
		service.SetName(request);

	public override Task<SetAddressResponse> SetAddress(SetAddressRequest request, ServerCallContext context) =>
		service.SetAddress(request);

	public override Task<DestroyResponse> Destroy(DestroyRequest request, ServerCallContext context) =>
		service.Destroy(request);
}
