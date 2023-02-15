using System;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using TimeKeep.RPC.Locations;
using static TimeKeep.RPC.Locations.LocationsService;

namespace TimeKeep.Client.Commands.Locations;

public class SetNameLocation : AsyncCommand<SetNameLocation.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, $"[{nameof(Id)}]")]
		public Guid? Id { get; init; }

		[CommandOption("--old-name")]
		public string? OldName { get; init; }

		[CommandOption("-n|--name")]
		public required string Name { get; init; }

		public SetNameRequest Request => Id is Guid guid
			? new() { Id = guid.ToString(), Name = Name }
			: new() { OldName = OldName, Name = Name };

		public override ValidationResult Validate() =>
			(Id is null ^ OldName is null)
				? ValidationResult.Error($"Must specify exactly one of {nameof(Id)} and {nameof(OldName)}")
				: ValidationResult.Success();

		public override string ToString()
		{
			if (Id is Guid guid)
			{
				return guid.ToString();
			}
			else
			{
				return OldName!;
			}
		}
	}

	private readonly LocationsServiceClient client;

	public SetNameLocation(LocationsServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		Console.Write($"Setting name of location \"{settings}\" to \"{settings.Name}\"... ");
		var response = await client.SetNameAsync(settings.Request);
		switch (response.Status)
		{
			case SetNameStatus.Success:
				Console.WriteLine("succeeded!");
				return 0;
			case SetNameStatus.LocationNotFound:
				Console.WriteLine("the location does not exists.");
				return 1;
			case SetNameStatus.LocationNameExists:
				Console.WriteLine("the location name already exists.");
				return 1;
			case SetNameStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}
}
