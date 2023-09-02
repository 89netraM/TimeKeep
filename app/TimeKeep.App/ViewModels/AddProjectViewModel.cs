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
using TimeKeep.RPC.Projects;
using System.Xml.Linq;

namespace TimeKeep.App.ViewModels;

public class AddProjectViewModel : ViewModelBase, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();

    [Reactive]
    public string Name { get; set; } = string.Empty;
    
    public ObservableCollection<string> Categories { get; } = new();
    public ObservableCollection<string> ExistingCategories { get; } = new();

    public ReactiveCommand<string, Unit> AddCategoryCommand;

    public ReactiveCommand<Unit, AddProjectResult> AddProjectCommand;

    public AddProjectViewModel()
    {
        this.WhenActivated(OnActivation);
        
        Categories.CollectionChanged += OnCategoriesCollectionChanged;
        AddCategoryCommand = ReactiveCommand.Create<string>(AddCategory);

        AddProjectCommand = ReactiveCommand.CreateFromTask(AddProject);
    }

    private void OnActivation(CompositeDisposable disposables)
    {
        var ctSource = new CancellationTokenSource();
        Disposable.Create(ctSource.Cancel).DisposeWith(disposables);
        ctSource.DisposeWith(disposables);

        LoadExistingCategories(ctSource.Token).DisposeWith(disposables);
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
            ExistingCategories.Add(category.Name);
        }
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
    
    private async Task<AddProjectResult> AddProject(CancellationToken ct)
    {
        try
        {
            var client = await RpcClientFactory.ProjectsClient;
            if (client is null)
            {
                return new AddProjectFailure("No entry client");
            }

            var request = new CreateRequest
            {
                Name = Name,
            };
            request.Categories.AddRange(Categories);

            var response = await client.CreateAsync(request, cancellationToken: ct);

            return response.Status switch
            {
                CreateStatus.Failure => new AddProjectFailure("Failed (???)"),
                CreateStatus.Success => new AddProjectSuccess(),
                CreateStatus.CategoryNotFound => new AddProjectFailure("One of the categories could not be found."),
                CreateStatus.ProjectExists => new AddProjectFailure($"The given project ({Name}) already exists."),
                var code => new AddProjectFailure($"Unknown response code: {code}"),
            };
        }
        catch (Exception e)
        {
            return new AddProjectFailure("Something went wrong:\n" + e.Message);
        }
    }
}

public abstract record AddProjectResult;

public record AddProjectSuccess : AddProjectResult;

public record AddProjectFailure(string Message) : AddProjectResult;
