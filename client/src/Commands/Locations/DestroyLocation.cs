using System;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using TimeKeep.RPC.Locations;
using static TimeKeep.RPC.Locations.LocationsService;

namespace TimeKeep.Client.Commands.Locations;

public class DestroyLocation : AsyncCommand<DestroyLocation.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandArgument(0, $"[{nameof(Id)}]")]
		public Guid? Id { get; init; }

		[CommandOption("-n|--name")]
		public string? Name { get; init; }

		public DestroyRequest Request => Id is Guid guid
			? new() { Id = guid.ToString() }
			: new() { Name = Name };

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

	public DestroyLocation(LocationsServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		Console.Write($"Destroying location \"{settings}\"... ");
		var response = await client.DestroyAsync(settings.Request);
		switch (response.Status)
		{
			case DestroyStatus.Success:
				Console.WriteLine("succeeded!");
				return 0;
			case DestroyStatus.LocationNotFound:
				Console.WriteLine("the location does not exists.");
				return 1;
			case DestroyStatus.LocationInUse:
				Console.WriteLine("the location is in use.");
				return 1;
			case DestroyStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}
}
