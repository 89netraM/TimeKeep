using System;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using TimeKeep.RPC.Projects;
using static TimeKeep.RPC.Projects.ProjectsService;

namespace TimeKeep.Client.Commands.Projects;

public class CreateProject : AsyncCommand<CreateProject.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, $"<{nameof(Name)}>")]
		public required string Name { get; init; }

		[CommandOption("-c|--categories")]
		public required string[] Categories { get; init; }
	}

	private readonly ProjectsServiceClient client;

	public CreateProject(ProjectsServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		Console.Write($"Creating project \"{settings.Name}\"... ");
		var request = new CreateRequest() { Name = settings.Name };
		request.Categories.AddRange(settings.Categories);
		var response = await client.CreateAsync(request);
		switch (response.Status)
		{
			case CreateStatus.Success:
				Console.WriteLine("succeeded!");
				return 0;
			case CreateStatus.ProjectExists:
				Console.WriteLine("the project already exists.");
				return 1;
			case CreateStatus.CategoryNotFound:
				Console.WriteLine("one of the categories could not be found.");
				return 1;
			case CreateStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}
}
