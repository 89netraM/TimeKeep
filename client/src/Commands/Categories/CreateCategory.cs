using System;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using TimeKeep.RPC.Categories;
using static TimeKeep.RPC.Categories.CategoriesService;

namespace TimeKeep.Client.Commands.Categories;

public class CreateCategory : AsyncCommand<CreateCategory.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, $"<{nameof(Name)}>")]
		public required string Name { get; init; }
	}

	private readonly CategoriesServiceClient client;

	public CreateCategory(CategoriesServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		Console.Write($"Creating category \"{settings.Name}\"... ");
		var response = await client.CreateAsync(new() { Name = settings.Name });
		switch (response.Status)
		{
			case CreateStatus.Success:
				Console.WriteLine("succeeded!");
				return 0;
			case CreateStatus.CategoryExists:
				Console.WriteLine("the category already exists.");
				return 1;
			case CreateStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}
}
