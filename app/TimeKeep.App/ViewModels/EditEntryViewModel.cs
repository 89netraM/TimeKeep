using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData.Binding;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TimeKeep.App.Services;
using CategoryListRequest = TimeKeep.RPC.Categories.ListRequest;
using CategoryOrder = TimeKeep.RPC.Categories.Order;
using TimeKeep.RPC.Entries;
using Project = TimeKeep.RPC.Projects.Project;
using ProjectListRequest = TimeKeep.RPC.Projects.ListRequest;
using ProjectOrder = TimeKeep.RPC.Projects.Order;
using Location = TimeKeep.RPC.Locations.Location;
using LocationListRequest = TimeKeep.RPC.Locations.ListRequest;
using LocationOrder = TimeKeep.RPC.Locations.Order;

namespace TimeKeep.App.ViewModels;

public class EditEntryViewModel : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();

    public string Title { get; }

    public string SaveVerb { get; }

    private readonly Entry? originalEntry;
    private readonly EditEntryDoneAction doneAction = EditEntryDoneAction.Create;

    [Reactive]
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.Now;

    [Reactive]
    public DateTimeOffset? EndTime { get; set; }

    public ObservableCollection<Project> Projects { get; } = new();
    [Reactive]
    public Project? SelectedProject { get; set; }

    public ObservableCollection<Location?> Locations { get; } = new();
    [Reactive]
    public Location? SelectedLocation { get; set; }

    public ObservableCollection<string> Categories { get; } = new();
    public ObservableCollection<string> ExistingCategories { get; } = new();

    public ReactiveCommand<string, Unit> AddCategoryCommand;

    public ReactiveCommand<Unit, EditEntryResult> DoneCommand;

    public EditEntryViewModel(Entry? entry)
	{
		Title = "New Entry";
		SaveVerb = "Add";

		if (entry is not null)
        {
			Title = "Edit Entry";
			SaveVerb = "Save";
            originalEntry = entry;
            doneAction = EditEntryDoneAction.Save;

			StartTime = entry.Start.ToDateTimeOffset().ToLocalTime();
            EndTime = entry.End?.ToDateTimeOffset().ToLocalTime();
            Categories = [.. entry.Categories];
        }

        this.WhenActivated(OnActivation);

        Changing.Where(e => e.PropertyName == nameof(SelectedProject)).Subscribe(OnSelectedProjectChanging);
        this.WhenPropertyChanged(t => t.SelectedProject, false).Subscribe(OnSelectedProjectChanged);

        Categories.CollectionChanged += OnCategoriesCollectionChanged;
        AddCategoryCommand = ReactiveCommand.Create<string>(AddCategory);

        DoneCommand = ReactiveCommand.CreateFromTask(Done);
    }

    private void OnActivation(CompositeDisposable disposables)
    {
        var ctSource = new CancellationTokenSource();
        Disposable.Create(ctSource.Cancel).DisposeWith(disposables);
        ctSource.DisposeWith(disposables);

        LoadExistingCategories(ctSource.Token).DisposeWith(disposables);
        LoadProjects(ctSource.Token).DisposeWith(disposables);
        LoadLocations(ctSource.Token).DisposeWith(disposables);
    }

    private async Task LoadExistingCategories(CancellationToken ct)
    {
        var client = await RpcClientFactory.CategoriesClient;
        if (ct.IsCancellationRequested || client is null)
        {
            return;
        }

        var response = client.List(new CategoryListRequest { Order = CategoryOrder.EntriesAsc }, cancellationToken: ct);
        await foreach (var category in response.ResponseStream.ReadAllAsync(ct))
        {
            if (!Categories.Contains(category.Name))
			{
				ExistingCategories.Add(category.Name);
			}
        }
    }

    private async Task LoadProjects(CancellationToken ct)
    {
        var client = await RpcClientFactory.ProjectsClient;
        if (ct.IsCancellationRequested || client is null)
        {
            return;
        }

        var response = client.List(new ProjectListRequest { Order = ProjectOrder.UsageDesc }, cancellationToken: ct);
        await foreach (var project in response.ResponseStream.ReadAllAsync(ct))
        {
            Projects.Add(project);
        }
    }

    private async Task LoadLocations(CancellationToken ct)
    {
        var client = await RpcClientFactory.LocationsClient;
        if (ct.IsCancellationRequested || client is null)
        {
            return;
        }

        var response = client.List(new LocationListRequest { Order = LocationOrder.UsageDesc }, cancellationToken: ct);
        Locations.Add(null);
        await foreach (var location in response.ResponseStream.ReadAllAsync(ct))
        {
            Locations.Add(location);
        }
        SelectedLocation = originalEntry?.Location;
	}

    private void OnCategoriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is not { Action: NotifyCollectionChangedAction.Remove, OldItems: { } removedItems })
        {
            return;
        }

        foreach (string item in removedItems)
        {
            ExistingCategories.Add(item);
        }
    }

    private void AddCategory(string category)
    {
        if (Categories.Contains(category))
        {
            return;
        }

        if (ExistingCategories.Contains(category))
        {
            Categories.Add(category);
            ExistingCategories.Remove(category);
        }
    }

    private void OnSelectedProjectChanging(IReactivePropertyChangedEventArgs<IReactiveObject> e)
    {
        if (SelectedProject is null)
        {
            return;
        }

        foreach (var category in SelectedProject.Categories)
        {
            Categories.Remove(category);
        }
    }

    private void OnSelectedProjectChanged(PropertyValue<EditEntryViewModel, Project?> e)
    {
        if (SelectedProject is null)
        {
            return;
        }

        foreach (var category in SelectedProject.Categories)
        {
            AddCategory(category);
        }
	}

    private Task<EditEntryResult> Done(CancellationToken ct) => doneAction switch
    {
        EditEntryDoneAction.Create => AddEntry(ct),
        EditEntryDoneAction.Save => SaveEntry(ct),
        _ => Task.FromResult((EditEntryResult)new EditEntryFailure($"Action not available: {doneAction}")),
	};

	private async Task<EditEntryResult> AddEntry(CancellationToken ct)
	{
		try
		{
			var client = await RpcClientFactory.EntriesClient;
			if (client is null)
			{
				return new EditEntryFailure("No entry client");
			}

			var request = new CreateRequest
			{
				Start = StartTime.ToUniversalTime().ToTimestamp(),
			};
			if (EndTime is DateTimeOffset endTime)
			{
				request.End = endTime.ToUniversalTime().ToTimestamp();
			}
			if (SelectedProject is not null)
			{
				request.Project = SelectedProject.Name;
			}
			request.Categories.AddRange(Categories);
			if (SelectedLocation is not null)
			{
				request.LocationId = SelectedLocation.Id;
			}

			var response = await client.CreateAsync(request, cancellationToken: ct);

			return response.Status switch
			{
				CreateStatus.Failure => new EditEntryFailure("Failed (???)"),
				CreateStatus.Success => new EditEntrySuccess(),
				CreateStatus.ProjectNotFound => new EditEntryFailure($"The given project ({SelectedProject?.Name}) could not be found."),
				CreateStatus.CategoryNotFound => new EditEntryFailure("One of the categories could not be found."),
				CreateStatus.LocationNotFound => new EditEntryFailure("The given location () could not be found."),
				var code => new EditEntryFailure($"Unknown response code: {code}"),
			};
		}
		catch (Exception e)
		{
			return new EditEntryFailure("Something went wrong:\n" + e.Message);
		}
	}

	private async Task<EditEntryResult> SaveEntry(CancellationToken ct)
    {
        try
        {
            var client = await RpcClientFactory.EntriesClient;
            if (client is null)
            {
                return new EditEntryFailure("No entry client");
            }

            var saveRequest = new CreateRequest
            {
                Start = StartTime.ToUniversalTime().ToTimestamp(),
            };
            if (EndTime is DateTimeOffset endTime)
            {
                saveRequest.End = endTime.ToUniversalTime().ToTimestamp();
            }
            if (SelectedProject is not null)
            {
                saveRequest.Project = SelectedProject.Name;
            }
            saveRequest.Categories.AddRange(Categories);
            if (SelectedLocation is not null)
            {
                saveRequest.LocationId = SelectedLocation.Id;
            }

            var createResponse = await client.CreateAsync(saveRequest, cancellationToken: ct);

            switch (createResponse.Status)
            {
                case CreateStatus.Failure: return new EditEntryFailure("Failed (???)");
                case CreateStatus.Success: break;
                case CreateStatus.ProjectNotFound: return new EditEntryFailure($"The given project ({SelectedProject?.Name}) could not be found.");
                case CreateStatus.CategoryNotFound: return new EditEntryFailure("One of the categories could not be found.");
                case CreateStatus.LocationNotFound: return new EditEntryFailure("The given location () could not be found.");
                case var code: return new EditEntryFailure($"Unknown response code: {code}");
            }

            var destroyRequest = new DestroyRequest
            {
                Id = originalEntry!.Id,
            };
            var destroyResponse = await client.DestroyAsync(destroyRequest, cancellationToken: ct);
			return destroyResponse.Status switch
			{
				DestroyStatus.Failure => new EditEntryFailure("Failed (???)"),
				DestroyStatus.Success => new EditEntrySuccess(),
                DestroyStatus.EntryNotFound => new EditEntryFailure("The original entry could not be found."),
				var code => new EditEntryFailure($"Unknown response code: {code}"),
			};
		}
        catch (Exception e)
        {
            return new EditEntryFailure("Something went wrong:\n" + e.Message);
        }
    }

    private enum EditEntryDoneAction
    {
        Create,
        Save,
    }
}

public abstract record EditEntryResult;

public record EditEntrySuccess : EditEntryResult;

public record EditEntryFailure(string Message) : EditEntryResult;
