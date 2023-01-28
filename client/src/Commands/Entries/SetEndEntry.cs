using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Spectre.Console;
using Spectre.Console.Cli;
using TimeKeep.RPC.Entries;
using static TimeKeep.RPC.Entries.EntriesService;

namespace TimeKeep.Client.Commands.Entries;

public class SetEndEntry : AsyncCommand<SetEndEntry.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, $"<{nameof(Id)}>")]
		public required EntrySpecification Id { get; init; }

		[CommandOption("-e|--end")]
		public required DateTime? End { get; init; }

		public override ValidationResult Validate()
		{
			if (Id is null)
			{
				return ValidationResult.Error("Entry Id is invalid, should be a UUID or \"^{index}\"");
			}

			return base.Validate();
		}
	}

	private readonly EntriesServiceClient client;
	private readonly EntrySpecificationService entrySpecificationService;

	public SetEndEntry(EntriesServiceClient client, EntrySpecificationService entrySpecificationService) =>
		(this.client, this.entrySpecificationService) = (client, entrySpecificationService);

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		var id = await entrySpecificationService.GetGuidOf(settings.Id);
		if (id is null)
		{
			Console.WriteLine("Unknown entry id");
			return 1;
		}

		var end = GetEnd(settings);
		Console.Write($"Setting end time to {end.ToLocalTime()}... ");
		var response = await client.SetEndAsync(new()
		{
			Id = id.ToString(),
			End = Timestamp.FromDateTime(end),
		});
		switch (response.Status)
		{
			case SetEndStatus.Success:
				Console.WriteLine("succeeded!");
				return 0;
			case SetEndStatus.EntryNotFound:
				Console.WriteLine("the entry does not exists.");
				return 1;
			case SetEndStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}

	private static DateTime GetEnd(Settings settings)
	{
		if (settings.End is DateTime end)
		{
			return end.ToUniversalTime();
		}
		else
		{
			return DateTime.UtcNow;
		}
	}
}
