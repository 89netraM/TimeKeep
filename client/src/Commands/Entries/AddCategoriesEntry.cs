using System;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using TimeKeep.RPC.Entries;
using static TimeKeep.RPC.Entries.EntriesService;

namespace TimeKeep.Client.Commands.Entries;

public class AddCategoriesEntry : AsyncCommand<AddCategoriesEntry.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, $"<{nameof(Id)}>")]
		public required EntrySpecification Id { get; init; }

		[CommandOption("-c|--categories")]
		public required string[] Categories { get; init; }

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

	public AddCategoriesEntry(EntriesServiceClient client, EntrySpecificationService entrySpecificationService) =>
		(this.client, this.entrySpecificationService) = (client, entrySpecificationService);

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		var id = await entrySpecificationService.GetGuidOf(settings.Id);
		if (id is null)
		{
			Console.WriteLine("Unknown entry id");
			return 1;
		}

		Console.Write($"Adding categories to entry \"{id}\"... ");
		var request = new AddCategoriesRequest() { Id = id.ToString() };
		request.Categories.AddRange(settings.Categories);
		var response = await client.AddCategoriesAsync(request);
		switch (response.Status)
		{
			case AddCategoriesStatus.Success:
				Console.WriteLine("succeeded!");
				return 0;
			case AddCategoriesStatus.EntryNotFound:
				Console.WriteLine("the entry does not exists.");
				return 1;
			case AddCategoriesStatus.CategoryNotFound:
				Console.WriteLine("one of the categories could not be found.");
				return 1;
			case AddCategoriesStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}
}
