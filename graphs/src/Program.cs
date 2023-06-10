using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TimeKeep.Graphs;
using static TimeKeep.RPC.Entries.EntriesService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Settings>(builder.Configuration.GetSection(Settings.Section));
builder.Services.AddSingleton<ChannelBase>(ChannelFactory.Factory);
builder.Services.AddSingleton<EntriesServiceClient>();
builder.Services.AddRazorPages(option => option.RootDirectory = "/src/Pages");

var app = builder.Build();

app.MapRazorPages();

app.Run();
