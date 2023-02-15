using System;
using System.Threading.Tasks;
using Grpc.Core;
using Spectre.Console.Cli;
using static TimeKeep.RPC.Locations.LocationsService;
using RPCLocation = TimeKeep.RPC.Locations.Location;

namespace TimeKeep.Client.Commands.Locations;

public class ListLocation : AsyncCommand
{
	private readonly LocationsServiceClient client;

	public ListLocation(LocationsServiceClient client) =>
		this.client = client;

	public override async Task<int> ExecuteAsync(CommandContext context)
	{
		Console.WriteLine("Fetching list of locations:");

		bool anyLocations = false;
		await foreach (var location in client.List(new()).ResponseStream.ReadAllAsync())
		{
			Console.WriteLine($"\t- {LocationToString(location)}");
			anyLocations = true;
		}

		if (!anyLocations)
		{
			Console.WriteLine("No locations found");
		}

		return 0;
	}

	private string LocationToString(RPCLocation location) =>
		$"{location.Address} ({(location.Name is string name ? name : location.Id.ToString())})";
}
