using System.IO;
using System.Reflection;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using TimeKeep.Client;
using TimeKeep.Client.Commands.Categories;
using TimeKeep.Client.Commands.Entries;
using TimeKeep.Client.Commands.Projects;
using TimeKeep.Client.Infrastructure;
using static TimeKeep.RPC.Categories.CategoriesService;
using static TimeKeep.RPC.Entries.EntriesService;
using static TimeKeep.RPC.Projects.ProjectsService;

var config = new ConfigurationBuilder()
	.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!)
	.AddJsonFile("appsettings.json", optional: true)
	.AddEnvironmentVariables()
	.Build();

var services = new ServiceCollection();
services.Configure<Settings>(config.GetSection(Settings.Section));
services.AddSingleton<ChannelBase>(ChannelFactory.Factory);
services.AddSingleton<EntrySpecificationService>();
services.AddSingleton<CategoriesServiceClient>();
services.AddSingleton<EntriesServiceClient>();
services.AddSingleton<ProjectsServiceClient>();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
	config.AddBranch("category", category =>
	{
		category.AddCommand<CreateCategory>("create");
		category.AddCommand<ListCategory>("list");
		category.AddCommand<DestroyCategory>("destroy");
	});
	config.AddBranch("entry", entry =>
	{
		entry.AddCommand<CreateEntry>("create");
		entry.AddCommand<ListEntry>("list");
		entry.AddCommand<GetEntry>("get");
		entry.AddCommand<AddCategoriesEntry>("add-category");
		entry.AddCommand<RemoveCategoriesEntry>("remove-category");
		entry.AddCommand<SetStartEntry>("set-start");
		entry.AddCommand<SetEndEntry>("set-end");
		entry.AddCommand<DestroyEntry>("destroy");
	});
	config.AddBranch("project", project =>
	{
		project.AddCommand<CreateProject>("create");
		project.AddCommand<ListProject>("list");
		project.AddCommand<AddCategoriesProject>("add-category");
		project.AddCommand<RemoveCategoriesProject>("remove-category");
		project.AddCommand<DestroyProject>("destroy");
	});

	config.AddCommand<CreateEntry>("start");
	config.AddCommand<ListEntry>("list");
	config.AddCommand<SetEndEntry>("end");
});

return app.Run(args);
