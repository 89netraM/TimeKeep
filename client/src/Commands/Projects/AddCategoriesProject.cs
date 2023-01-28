using System;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using TimeKeep.RPC.Projects;
using static TimeKeep.RPC.Projects.ProjectsService;

namespace TimeKeep.Client.Commands.Projects;

public class AddCategoriesProject : AsyncCommand<AddCategoriesProject.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, $"<{nameof(Name)}>")]
		public required string Name { get; init; }

		[CommandOption("-c|--categories")]
		public required string[] Categories { get; init; }
	}

	private readonly ProjectsServiceClient client;

	public AddCategoriesProject(ProjectsServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		Console.Write($"Adding categories to project \"{settings.Name}\"... ");
		var request = new AddCategoriesRequest() { Name = settings.Name };
		request.Categories.AddRange(settings.Categories);
		var response = await client.AddCategoriesAsync(request);
		switch (response.Status)
		{
			case AddCategoriesStatus.Success:
				Console.WriteLine("succeeded!");
				return 0;
			case AddCategoriesStatus.ProjectNotFound:
				Console.WriteLine("the project does not exists.");
				return 1;
			case AddCategoriesStatus.CategoryNotFound:
				Console.WriteLine("one of the categories could not be found.");
				return 1;
			case AddCategoriesStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}
}
