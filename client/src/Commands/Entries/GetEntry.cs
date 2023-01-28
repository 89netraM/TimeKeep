using System;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using TimeKeep.RPC.Entries;
using static TimeKeep.RPC.Entries.EntriesService;

namespace TimeKeep.Client.Commands.Entries;

public class GetEntry : AsyncCommand<GetEntry.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, $"<{nameof(Id)}>")]
		public required EntrySpecification Id { get; init; }

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

	public GetEntry(EntriesServiceClient client, EntrySpecificationService entrySpecificationService) =>
		(this.client, this.entrySpecificationService) = (client, entrySpecificationService);

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		var id = await entrySpecificationService.GetGuidOf(settings.Id);
		if (id is null)
		{
			Console.WriteLine("Unknown entry id");
			return 1;
		}

		Console.Write($"Fetching entry {id}... ");
		var response = await client.GetAsync(new() { Id = id.ToString() });
		switch (response.Status)
		{
			case GetStatus.Success:
				Console.WriteLine();
				Console.WriteLine($"\tStart:      {response.Entry.Start.ToDateTime().ToLocalTime()}");
				Console.WriteLine($"\tEnd:        {response.Entry.End?.ToDateTime().ToLocalTime()}");
				Console.WriteLine($"\tCategories: {String.Join(", ", response.Entry.Categories)}");
				return 0;
			case GetStatus.EntryNotFound:
				Console.WriteLine("the entry does not exists.");
				return 1;
			case GetStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}
}
