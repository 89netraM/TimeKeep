using System;
using System.Threading.Tasks;
using Grpc.Core;
using Spectre.Console.Cli;
using static TimeKeep.RPC.Categories.CategoriesService;

namespace TimeKeep.Client.Commands.Categories;

public class ListCategory : AsyncCommand
{
	private readonly CategoriesServiceClient client;

	public ListCategory(CategoriesServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context)
	{
		Console.WriteLine("Fetching list of categories:");

		bool anyCategories = false;
		await foreach (var category in client.List(new()).ResponseStream.ReadAllAsync())
		{
			Console.WriteLine($"\t- {category.Name}");
			anyCategories = true;
		}

		if (!anyCategories)
		{
			Console.WriteLine("No categories found");
		}

		return 0;
	}
}
