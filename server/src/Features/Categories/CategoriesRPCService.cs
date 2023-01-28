using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TimeKeep.RPC.Categories;
using static TimeKeep.RPC.Categories.CategoriesService;

namespace TimeKeep.Features.Categories;

[Authorize]
public class CategoriesRPCService : CategoriesServiceBase
{
	private readonly CategoriesService service;

	public CategoriesRPCService(CategoriesService service) =>
		this.service = service;

	public override async Task List(ListRequest request, IServerStreamWriter<Category> responseStream, ServerCallContext context)
	{
		await foreach (var category in service.List(request).AsAsyncEnumerable())
		{
			if (context.CancellationToken.IsCancellationRequested)
			{
				break;
			}
			await responseStream.WriteAsync(category);
			if (context.CancellationToken.IsCancellationRequested)
			{
				break;
			}
		}
	}

	public override Task<CreateResponse> Create(CreateRequest request, ServerCallContext context) =>
		service.Create(request);

	public override Task<DestroyResponse> Destroy(DestroyRequest request, ServerCallContext context) =>
		service.Destroy(request);
}
