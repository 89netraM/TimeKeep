using Grpc.Core;
using Grpc.Net.Client;
using System.Text;
using System;
using System.Threading.Tasks;
using TimeKeep.RPC.Entries;
using Splat;
using TimeKeep.RPC.Categories;
using TimeKeep.RPC.Locations;
using TimeKeep.RPC.Projects;

namespace TimeKeep.App.Services;

public class RpcClientFactory
{
    private static readonly Lazy<Task<EntriesService.EntriesServiceClient?>> entriesClient =
        new(() => CreateClient(c => new EntriesService.EntriesServiceClient(c)));
    public static Task<EntriesService.EntriesServiceClient?> EntriesClient => entriesClient.Value;

    private static readonly Lazy<Task<CategoriesService.CategoriesServiceClient?>> categoriesClient =
        new(() => CreateClient(c => new CategoriesService.CategoriesServiceClient(c)));
    public static Task<CategoriesService.CategoriesServiceClient?> CategoriesClient => categoriesClient.Value;

    private static readonly Lazy<Task<ProjectsService.ProjectsServiceClient?>> projectsClient =
        new(() => CreateClient(c => new ProjectsService.ProjectsServiceClient(c)));
    public static Task<ProjectsService.ProjectsServiceClient?> ProjectsClient => projectsClient.Value;

    private static readonly Lazy<Task<LocationsService.LocationsServiceClient?>> locationsClient =
        new(() => CreateClient(c => new LocationsService.LocationsServiceClient(c)));
    public static Task<LocationsService.LocationsServiceClient?> LocationsClient => locationsClient.Value;

    private static async Task<T?> CreateClient<T>(Func<ChannelBase, T> factory)
        where T : class =>
        await channel.Value is not ChannelBase c ? null : factory(c);

    private static readonly Lazy<Task<ChannelBase?>> channel = new(CreateChannel);

    private static async Task<ChannelBase?> CreateChannel()
    {
        var (server, token) = await LoadServerAndToken();
        if (server is null || token is null)
        {
            return null;
        }

        return ChannelFactory(server, token);
    }

    private static async Task<(string?, string?)> LoadServerAndToken()
    {
        var settingsService = Locator.Current.GetService<ISettingsService>();
        if (settingsService is null)
        {
            return (null, null);
        }

        var server = await settingsService.GetSettingAsync("Server");
        var token = await settingsService.GetSettingAsync("Token");
        return (server, token);
    }

    private static ChannelBase ChannelFactory(string server, string token)
    {
        var base64Token = GetBase64Token(token);
        var credentials = CallCredentials.FromInterceptor((context, metadata) =>
        {
            metadata.Add("Authorization", $"Bearer {base64Token}");
            return Task.CompletedTask;
        });

        var channel = GrpcChannel.ForAddress(FormatUrl(server), new GrpcChannelOptions
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

    private static Uri FormatUrl(string server)
    {
        if (!Uri.IsWellFormedUriString(server, UriKind.Absolute))
        {
            server = Uri.UriSchemeHttps + Uri.SchemeDelimiter + server;
        }

        return new Uri(server);
    }
}
