using System;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using TimeKeep.RPC.Categories;
using TimeKeep.RPC.Entries;
using TimeKeep.Stats.Options;

namespace TimeKeep.Stats;

public static class RpcServiceCollectionExtensions
{
    public static IServiceCollection AddRpcClients(
        this IServiceCollection services,
        TimeKeepOptions options
    )
    {
        var base64Token = GetBase64Token(options.Token);
        var callCredentials = CallCredentials.FromInterceptor(
            (_, metadata) =>
            {
                metadata.Add("Authorization", $"Bearer {base64Token}");
                return Task.CompletedTask;
            }
        );
        services
            .AddGrpcClient<EntriesService.EntriesServiceClient>(
                (sp, o) => o.Address = new(options.Server)
            )
            .AddCallCredentials(callCredentials);
        services
            .AddGrpcClient<CategoriesService.CategoriesServiceClient>(
                (sp, o) => o.Address = new(options.Server)
            )
            .AddCallCredentials(callCredentials);
        return services;
    }

    private static string GetBase64Token(string token)
    {
        var utf8ValidatingEncoding = new UTF8Encoding(false, true);
        var tokenBytes = utf8ValidatingEncoding.GetBytes(token);
        return Convert.ToBase64String(tokenBytes);
    }
}
