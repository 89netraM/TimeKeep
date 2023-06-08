using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using static TimeKeep.RPC.Entries.EntriesService;

namespace TimeKeep.Graphs;

public static class ServiceFactory
{
	private static readonly Lazy<ChannelBase> channelBase = new(GetChannelBase);

	public static Lazy<EntriesServiceClient> EntryService { get; } =
		new(() => new EntriesServiceClient(channelBase.Value));

	private static ChannelBase GetChannelBase()
	{
		var settings = GetSettings();

		var base64Token = ToBase64(settings.token);
		var credentials = CallCredentials.FromInterceptor((context, metadata) =>
		{
			metadata.Add("Authorization", $"Bearer {base64Token}");
			return Task.CompletedTask;
		});
		var channel = GrpcChannel.ForAddress(settings.url, new GrpcChannelOptions
		{
			Credentials = ChannelCredentials.Create(ChannelCredentials.SecureSsl, credentials),
		});
		return channel;

		static (string token, string url) GetSettings()
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!)
				.AddJsonFile("appsettings.json", optional: true)
				.AddEnvironmentVariables()
				.Build();
			var timeKeepSettings = config.GetSection("TimeKeep");
			return (
				GetStringOrThrow(timeKeepSettings, "Token"),
				GetStringOrThrow(timeKeepSettings, "Url")
			);
		}

		static string GetStringOrThrow(IConfigurationSection section, string property) =>
			section.GetValue<string>(property) ?? throw new ArgumentNullException(property);

		static string ToBase64(string value)
		{
			var utf8ValidatingEncoding = new UTF8Encoding(false, true);
			var tokenBytes = utf8ValidatingEncoding.GetBytes(value);
			return Convert.ToBase64String(tokenBytes);
		}
	}
}
