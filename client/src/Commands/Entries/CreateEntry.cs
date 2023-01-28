using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Spectre.Console.Cli;
using TimeKeep.RPC.Entries;
using static TimeKeep.RPC.Entries.EntriesService;

namespace TimeKeep.Client.Commands.Entries;

public class CreateEntry : AsyncCommand<CreateEntry.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandOption("-s|--start")]
		public DateTime? Start { get; init; } = null;

		[CommandOption("-e|--end")]
		public DateTime? End { get; init; } = null;

		[CommandOption("-p|--project")]
		public string? Project { get; init; } = null;

		[CommandOption("-c|--categories")]
		public string[] Categories { get; init; } = Array.Empty<string>();
	}

	private readonly EntriesServiceClient client;

	public CreateEntry(EntriesServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		var start = GetStart(settings);
		Console.Write($"Creating entry starting {start.ToLocalTime()}... ");
		var request = new CreateRequest()
		{
			Start = Timestamp.FromDateTime(start),
			End = settings.End is DateTime end ? Timestamp.FromDateTime(end.ToUniversalTime()) : null,
		};
		if (settings.Project is not null)
		{
			request.Project = settings.Project;
		}
		request.Categories.AddRange(settings.Categories);
		var response = await client.CreateAsync(request);
		switch (response.Status)
		{
			case CreateStatus.Success:
				Console.WriteLine($"succeeded, with id {response.Id}.");
				return 0;
			case CreateStatus.ProjectNotFound:
				Console.WriteLine("the project does not exists.");
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

	private static DateTime GetStart(Settings settings)
	{
		if (settings.Start is DateTime start)
		{
			return start.ToUniversalTime();
		}
		else
		{
			return DateTime.UtcNow;
		}
	}
}
