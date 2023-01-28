using System;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using TimeKeep.RPC.Projects;
using static TimeKeep.RPC.Projects.ProjectsService;

namespace TimeKeep.Client.Commands.Projects;

public class RemoveCategoriesProject : AsyncCommand<RemoveCategoriesProject.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, $"<{nameof(Name)}>")]
		public required string Name { get; init; }

		[CommandOption("-c|--categories")]
		public required string[] Categories { get; init; }
	}

	private readonly ProjectsServiceClient client;

	public RemoveCategoriesProject(ProjectsServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		Console.Write($"Removing categories from project \"{settings.Name}\"... ");
		var request = new RemoveCategoriesRequest() { Name = settings.Name };
		request.Categories.AddRange(settings.Categories);
		var response = await client.RemoveCategoriesAsync(request);
		switch (response.Status)
		{
			case RemoveCategoriesStatus.Success:
				Console.WriteLine("succeeded!");
				return 0;
			case RemoveCategoriesStatus.ProjectNotFound:
				Console.WriteLine("the project does not exists.");
				return 1;
			case RemoveCategoriesStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}
}
