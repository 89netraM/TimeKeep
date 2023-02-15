using System;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using TimeKeep.RPC.Entries;
using static TimeKeep.RPC.Entries.EntriesService;

namespace TimeKeep.Client.Commands.Entries;

public class SetLocationEntry : AsyncCommand<SetLocationEntry.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, $"<{nameof(Id)}>")]
		public required EntrySpecification Id { get; init; }

		[CommandOption("--location-name")]
		public string? LocationName { get; init; }

		[CommandOption("--location-id")]
		public Guid? LocationId { get; init; }

		[CommandOption("--unset")]
		public bool Unset { get; init; } = false;

		public override ValidationResult Validate()
		{
			if (Id is null)
			{
				return ValidationResult.Error("Entry Id is invalid, should be a UUID or \"^{index}\"");
			}

			if (new bool[] { LocationName is not null, LocationId is not null, Unset }.Count(p => p) != 1)
			{
				return ValidationResult.Error($"Must specify exactly one of {nameof(LocationName)}, {nameof(LocationId)}, and {nameof(Unset)}");
			}

			return base.Validate();
		}

		public string ToLocationString()
		{
			if (LocationName is string locationName)
			{
				return locationName;
			}
			else if (LocationId is Guid locationId)
			{
				return locationId.ToString();
			}
			else
			{
				return "null";
			}
		}
	}

	private readonly EntriesServiceClient client;
	private readonly EntrySpecificationService entrySpecificationService;

	public SetLocationEntry(EntriesServiceClient client, EntrySpecificationService entrySpecificationService) =>
		(this.client, this.entrySpecificationService) = (client, entrySpecificationService);

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		var id = await entrySpecificationService.GetGuidOf(settings.Id);
		if (id is null)
		{
			Console.WriteLine("Unknown entry id");
			return 1;
		}

		var request = new SetLocationRequest() { Id = id.ToString(), };
		if (settings.LocationName is string locationName)
		{
			request.LocationName = locationName;
		}
		else if (settings.LocationId is Guid locationId)
		{
			request.LocationId = locationId.ToString();
		}

		Console.Write($"Setting location of entry \"{id}\" to \"{settings.ToLocationString()}\"... ");
		var response = await client.SetLocationAsync(request);
		switch (response.Status)
		{
			case SetLocationStatus.Success:
				Console.WriteLine("succeeded!");
				return 0;
			case SetLocationStatus.LocationNotFound:
				Console.WriteLine("the location does not exists.");
				return 1;
			case SetLocationStatus.EntryNotFound:
				Console.WriteLine("the entry does not exists.");
				return 1;
			case SetLocationStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}
}
