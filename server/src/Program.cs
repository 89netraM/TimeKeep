using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimeKeep.Authentication;
using TimeKeep.Features.Categories;
using TimeKeep.Features.Entries;
using TimeKeep.Features.Locations;
using TimeKeep.Features.Projects;
using TimeKeep.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.Configure<BearerAuthenticationSettings>(builder.Configuration.GetSection(BearerAuthenticationSettings.Section));
builder.Services.AddAuthentication()
	.AddScheme<BearerAuthenticationOptions, BearerAuthentication>(BearerAuthentication.Scheme, configureOptions: null);
builder.Services.AddAuthorization();

builder.Services.AddDbContext<TimeKeepContext>();
builder.Services.AddTransient<CategoriesService>();
builder.Services.AddTransient<EntriesService>();
builder.Services.AddTransient<ProjectsService>();
builder.Services.AddTransient<LocationsService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<CategoriesRPCService>();
app.MapGrpcService<EntriesRPCService>();
app.MapGrpcService<ProjectsRPCService>();
app.MapGrpcService<LocationsRPCService>();

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<TimeKeepContext>();
	db.Database.Migrate();
}

app.Run();
