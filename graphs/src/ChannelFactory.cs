using System;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TimeKeep.Graphs;

public static class ChannelFactory
{
	public static ChannelBase Factory(IServiceProvider services)
	{
		var settings = services.GetRequiredService<IOptions<Settings>>().Value;

		var base64Token = GetBase64Token(settings.Token);
		var credentials = CallCredentials.FromInterceptor((context, metadata) =>
		{
			metadata.Add("Authorization", $"Bearer {base64Token}");
			return Task.CompletedTask;
		});

		var channel = GrpcChannel.ForAddress(settings.Url, new GrpcChannelOptions
		{
			Credentials = ChannelCredentials.Create(ChannelCredentials.SecureSsl, credentials),
		});
		return channel;
	}

	private static string GetBase64Token(string token)
	{
		var utf8ValidatingEncoding = new UTF8Encoding(false, true);
		var tokenBytes = utf8ValidatingEncoding.GetBytes(token);
		return Convert.ToBase64String(tokenBytes);
	}
}
