using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TimeKeep.RPC.Projects;
using static TimeKeep.RPC.Projects.ProjectsService;

namespace TimeKeep.Features.Projects;

[Authorize]
public class ProjectsRPCService : ProjectsServiceBase
{
	private readonly ProjectsService service;

	public ProjectsRPCService(ProjectsService service) =>
		this.service = service;

	public override async Task List(ListRequest request, IServerStreamWriter<Project> responseStream, ServerCallContext context)
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

	public override Task<AddCategoriesResponse> AddCategories(AddCategoriesRequest request, ServerCallContext context) =>
		service.AddCategories(request);

	public override Task<RemoveCategoriesResponse> RemoveCategories(RemoveCategoriesRequest request, ServerCallContext context) =>
		service.RemoveCategories(request);

	public override Task<DestroyResponse> Destroy(DestroyRequest request, ServerCallContext context) =>
		service.Destroy(request);
}
