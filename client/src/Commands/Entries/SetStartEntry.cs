using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Spectre.Console;
using Spectre.Console.Cli;
using TimeKeep.RPC.Entries;
using static TimeKeep.RPC.Entries.EntriesService;

namespace TimeKeep.Client.Commands.Entries;

public class SetStartEntry : AsyncCommand<SetStartEntry.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, $"<{nameof(Id)}>")]
		public required EntrySpecification Id { get; init; }

		[CommandOption("-s|--start")]
		public required DateTime Start { get; init; }

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

	public SetStartEntry(EntriesServiceClient client, EntrySpecificationService entrySpecificationService) =>
		(this.client, this.entrySpecificationService) = (client, entrySpecificationService);

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		var id = await entrySpecificationService.GetGuidOf(settings.Id);
		if (id is null)
		{
			Console.WriteLine("Unknown entry id");
			return 1;
		}

		Console.Write($"Setting start time to {settings.Start.ToLocalTime()}... ");
		var response = await client.SetStartAsync(new()
		{
			Id = id.ToString(),
			Start = Timestamp.FromDateTime(settings.Start.ToLocalTime()),
		});
		switch (response.Status)
		{
			case SetStartStatus.Success:
				Console.WriteLine("succeeded!");
				return 0;
			case SetStartStatus.EntryNotFound:
				Console.WriteLine("the entry does not exists.");
				return 1;
			case SetStartStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}
}
