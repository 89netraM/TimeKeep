using System;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using TimeKeep.RPC.Locations;
using static TimeKeep.RPC.Locations.LocationsService;

namespace TimeKeep.Client.Commands.Locations;

public class SetAddressLocation : AsyncCommand<SetAddressLocation.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, $"[{nameof(Id)}]")]
		public Guid? Id { get; init; }

		[CommandOption("--old-name")]
		public string? Name { get; init; }

		[CommandOption("-a|--address")]
		public required string Address { get; init; }

		public SetAddressRequest Request => Id is Guid guid
			? new() { Id = guid.ToString(), Address = Address }
			: new() { Name = Name, Address = Address };

		public override ValidationResult Validate() =>
			(Id is null ^ Name is null)
				? ValidationResult.Error($"Must specify exactly one of {nameof(Id)} and {nameof(Name)}")
				: ValidationResult.Success();

		public override string ToString()
		{
			if (Id is Guid guid)
			{
				return guid.ToString();
			}
			else
			{
				return Name!;
			}
		}
	}

	private readonly LocationsServiceClient client;

	public SetAddressLocation(LocationsServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		Console.Write($"Setting address of location \"{settings}\" to \"{settings.Address}\"... ");
		var response = await client.SetAddressAsync(settings.Request);
		switch (response.Status)
		{
			case SetAddressStatus.Success:
				Console.WriteLine("succeeded!");
				return 0;
			case SetAddressStatus.LocationNotFound:
				Console.WriteLine("the location does not exists.");
				return 1;
			case SetAddressStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}
}
