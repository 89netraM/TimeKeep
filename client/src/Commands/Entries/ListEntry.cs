using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Spectre.Console;
using Spectre.Console.Cli;
using TimeKeep.RPC.Entries;
using static TimeKeep.RPC.Entries.EntriesService;
using Duration = TimeKeep.Common.Duration;

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

			if (After is DateTime after && Before is DateTime before && after >= before)
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
		
		var request = new ListRequest()
		{
			EndStatus = GetEndStatus(settings),
			Order = settings.Order,
			After = settings.After is DateTime after ? Timestamp.FromDateTime(after.ToUniversalTime()) : null,
			Before = settings.Before is DateTime before ? Timestamp.FromDateTime(before.ToUniversalTime()) : null,
		};
		request.Categories.AddRange(settings.Categories);

		var duration = new Duration();
		await foreach (var entry in client.List(request).ResponseStream.ReadAllAsync())
		{
			var start = entry.Start.ToDateTime().ToLocalTime();
			var end = entry.End?.ToDateTime().ToLocalTime();
			Console.WriteLine($"\t- {entry.Id} ({start} - {end})");
			if (entry.Location is not null)
			{
				if (entry.Location.HasName)
				{
					Console.WriteLine($"\t  {entry.Location.Name}");
				}
				else
				{
					Console.WriteLine($"\t  {entry.Location.Address} ({entry.Location.Id})");
				}
			}
			duration.AddInterval(start, end ?? DateTime.Now);
		}

		var totalDuration = duration.TotalDuration;
		if (totalDuration > TimeSpan.Zero)
		{
			Console.WriteLine($"Total time spent: {totalDuration}");
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
