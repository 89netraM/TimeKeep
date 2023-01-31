using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Spectre.Console;
using Spectre.Console.Cli;
using TimeKeep.RPC.Entries;
using static TimeKeep.RPC.Entries.EntriesService;

namespace TimeKeep.Client.Commands.Entries;

public class ListEntry : AsyncCommand<ListEntry.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandOption("-a|--only-active")]
		public bool OnlyActive { get; init; }

		[CommandOption("--only-completed")]
		public bool OnlyCompleted { get; init; }

		[CommandOption("-o|--order")]
		public Order Order { get; init; }

		[CommandOption("-c|--categories")]
		public string[] Categories { get; init; } = Array.Empty<string>();

		[CommandOption("--after")]
		public DateTime? After { get; init; }

		[CommandOption("--before")]
		public DateTime? Before { get; init; }

		public override ValidationResult Validate()
		{
			if (OnlyActive && OnlyCompleted)
			{
				return ValidationResult.Error("Both --only-active and --only-completed can not be specified together");
			}

			if (After is DateTime after && Before is DateTime before && after < before)
			{
				return ValidationResult.Error("When both --after and --before is specified \"after\" must before \"before\"");
			}

			return base.Validate();
		}
	}

	private readonly EntriesServiceClient client;

	public ListEntry(EntriesServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		Console.WriteLine("Fetching list of entries:");

		bool anyEntries = false;
		var request = new ListRequest()
		{
			EndStatus = GetEndStatus(settings),
			Order = settings.Order,
			After = settings.After is DateTime after ? Timestamp.FromDateTime(after.ToUniversalTime()) : null,
			Before = settings.Before is DateTime before ? Timestamp.FromDateTime(before.ToUniversalTime()) : null,
		};
		request.Categories.AddRange(settings.Categories);

		var totalTime = TimeSpan.Zero;
		await foreach (var entry in client.List(request).ResponseStream.ReadAllAsync())
		{
			var start = entry.Start.ToDateTime().ToLocalTime();
			var end = entry.End?.ToDateTime().ToLocalTime();
			Console.WriteLine($"\t- {entry.Id} ({start} - {end})");
			totalTime += (end ?? DateTime.Now) - start;
			anyEntries = true;
		}

		if (anyEntries)
		{
			Console.WriteLine($"Total time spent: {totalTime}");
		}
		else
		{
			Console.WriteLine("No entries found");
		}

		return 0;
	}

	private static EndStatus GetEndStatus(Settings settings)
	{
		if (settings.OnlyCompleted)
		{
			return EndStatus.OnlyCompleted;
		}
		else if (settings.OnlyActive)
		{
			return EndStatus.OnlyActive;
		}
		else
		{
			return EndStatus.All;
		}
	}
}
