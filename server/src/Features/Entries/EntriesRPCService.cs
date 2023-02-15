using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TimeKeep.RPC.Entries;
using static TimeKeep.RPC.Entries.EntriesService;

namespace TimeKeep.Features.Entries;

[Authorize]
public class EntriesRPCService : EntriesServiceBase
{
	private readonly EntriesService service;

	public EntriesRPCService(EntriesService service) =>
		this.service = service;

	public override async Task List(ListRequest request, IServerStreamWriter<Entry> responseStream, ServerCallContext context)
	{
		await foreach (var entry in service.List(request).AsAsyncEnumerable())
		{
			if (context.CancellationToken.IsCancellationRequested)
			{
				break;
			}
			var rpcEntry = new Entry
			{
				Id = entry.Id.ToString(),
				Start = Timestamp.FromDateTime(entry.Start),
				End = entry.End is DateTime end ? Timestamp.FromDateTime(end) : null,
			};
			if (entry.Location is ResponseLocation location)
			{
				rpcEntry.Location = new()
				{
					Id = location.Id.ToString(),
					Name = location.Name,
					Address = location.Address,
				};
			}
			rpcEntry.Categories.AddRange(entry.Categories);
			await responseStream.WriteAsync(rpcEntry);
			if (context.CancellationToken.IsCancellationRequested)
			{
				break;
			}
		}
	}

	public override Task<GetResponse> Get(GetRequest request, ServerCallContext context) =>
		service.Get(request);

	public override Task<CreateResponse> Create(CreateRequest request, ServerCallContext context) =>
		service.Create(request);

	public override Task<SetStartResponse> SetStart(SetStartRequest request, ServerCallContext context) =>
		service.SetStart(request);

	public override Task<SetEndResponse> SetEnd(SetEndRequest request, ServerCallContext context) =>
		service.SetEnd(request);

	public override Task<AddCategoriesResponse> AddCategories(AddCategoriesRequest request, ServerCallContext context) =>
		service.AddCategories(request);

	public override Task<RemoveCategoriesResponse> RemoveCategories(RemoveCategoriesRequest request, ServerCallContext context) =>
		service.RemoveCategories(request);

	public override Task<SetLocationResponse> SetLocation(SetLocationRequest request, ServerCallContext context) =>
		service.SetLocation(request);

	public override Task<DestroyResponse> Destroy(DestroyRequest request, ServerCallContext context) =>
		service.Destroy(request);
}
