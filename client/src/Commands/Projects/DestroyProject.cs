using System;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using TimeKeep.RPC.Projects;
using static TimeKeep.RPC.Projects.ProjectsService;

namespace TimeKeep.Client.Commands.Projects;

public class DestroyProject : AsyncCommand<DestroyProject.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, $"<{nameof(Name)}>")]
		public required string Name { get; init; }
	}

	private readonly ProjectsServiceClient client;

	public DestroyProject(ProjectsServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		Console.Write($"Destroying project \"{settings.Name}\"... ");
		var response = await client.DestroyAsync(new() { Name = settings.Name });
		switch (response.Status)
		{
			case DestroyStatus.Success:
				Console.WriteLine("succeeded!");
				return 0;
			case DestroyStatus.ProjectNotFound:
				Console.WriteLine("the project does not exists.");
				return 1;
			case DestroyStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}
}
