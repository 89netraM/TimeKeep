using System;
using System.Threading.Tasks;
using Grpc.Core;
using Spectre.Console.Cli;
using static TimeKeep.RPC.Projects.ProjectsService;

namespace TimeKeep.Client.Commands.Projects;

public class ListProject : AsyncCommand
{
	private readonly ProjectsServiceClient client;

	public ListProject(ProjectsServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context)
	{
		Console.WriteLine("Fetching list of projects:");

		bool anyProjects = false;
		await foreach (var category in client.List(new()).ResponseStream.ReadAllAsync())
		{
			Console.WriteLine($"\t- {category.Name}");
			anyProjects = true;
		}

		if (!anyProjects)
		{
			Console.WriteLine("No projects found");
		}

		return 0;
	}
}
