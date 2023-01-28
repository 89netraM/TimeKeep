using System;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using TimeKeep.RPC.Categories;
using static TimeKeep.RPC.Categories.CategoriesService;

namespace TimeKeep.Client.Commands.Categories;

public class DestroyCategory : AsyncCommand<DestroyCategory.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, $"<{nameof(Name)}>")]
		public required string Name { get; init; }
	}

	private readonly CategoriesServiceClient client;

	public DestroyCategory(CategoriesServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		Console.Write($"Destroying category \"{settings.Name}\"... ");
		var response = await client.DestroyAsync(new() { Name = settings.Name });
		switch (response.Status)
		{
			case DestroyStatus.Success:
				Console.WriteLine("succeeded!");
				return 0;
			case DestroyStatus.CategoryNotFound:
				Console.WriteLine("the category does not exists.");
				return 1;
			case DestroyStatus.CategoryInUse:
				Console.WriteLine("the category is in use.");
				return 1;
			case DestroyStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}
}
