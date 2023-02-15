using System;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using TimeKeep.RPC.Locations;
using static TimeKeep.RPC.Locations.LocationsService;

namespace TimeKeep.Client.Commands.Locations;

public class CreateLocation : AsyncCommand<CreateLocation.Settings>
{
	public class Settings : CommandSettings
	{
		[CommandOption("-n|--name")]
		public string? Name { get; init; }

		[CommandOption("-a|--address")]
		public required string Address { get; init; }


		public string Identifier => Name ?? Address;
	}

	private readonly LocationsServiceClient client;

	public CreateLocation(LocationsServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
	{
		var request = new CreateRequest()
		{
			Address = settings.Address,
		};
		if (settings.Name is string name)
		{
			request.Name = name;
		}
		Console.Write($"Creating location \"{settings.Identifier}\"... ");
		var response = await client.CreateAsync(request);
		switch (response.Status)
		{
			case CreateStatus.Success:
				Console.WriteLine("succeeded!");
				return 0;
			case CreateStatus.LocationNameExists:
				Console.WriteLine("the locations name already exists.");
				return 1;
			case CreateStatus.Failure:
			default:
				Console.WriteLine("an unknown error occurred.");
				return 1;
		}
	}
}
