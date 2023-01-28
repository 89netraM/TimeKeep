using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeKeep.Models;
using TimeKeep.RPC.Categories;
using RPCCategory = TimeKeep.RPC.Categories.Category;

namespace TimeKeep.Features.Categories;

public class CategoriesService
{
	private readonly TimeKeepContext context;

	public CategoriesService(TimeKeepContext context) =>
		this.context = context;

	public IQueryable<RPCCategory> List(ListRequest request)
	{
		IQueryable<Models.Category> query = context.Categories.AsNoTracking();

		query = request.Order switch
		{
			Order.EntriesAsc => query.OrderBy(category => category.Projects.Count()),
			Order.EntriesDesc => query.OrderByDescending(category => category.Projects.Count()),
		};

		return query
			.Select(category => new RPCCategory { Name = category.Name });
	}

	public async Task<CreateResponse> Create(CreateRequest request)
	{
		var category = new Models.Category
		{
			Name = request.Name,
		};
		await context.Categories.AddAsync(category);
		await context.SaveChangesAsync();
		return new CreateResponse { Status = CreateStatus.Success };
	}

	public async Task<DestroyResponse> Destroy(DestroyRequest request)
	{
		var category = await context.Categories.FindAsync(request.Name);
		if (category is null)
		{
			return new DestroyResponse { Status = DestroyStatus.CategoryNotFound };
		}
		context.Categories.Remove(category);
		await context.SaveChangesAsync();
		return new DestroyResponse { Status = DestroyStatus.Success };
	}
}
