using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimeKeep.Stats;
using TimeKeep.Stats.Options;
using TimeKeep.Stats.Services;

var builder = WebApplication.CreateBuilder(args);

var timeKeepOptions =
    builder.Configuration.GetSection("TimeKeep").Get<TimeKeepOptions>()
    ?? throw new Exception("Missing \"TimeKeep\" options.");

builder.Services.AddRpcClients(timeKeepOptions);

builder.Services.AddTransient<WeekService>();

builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();
