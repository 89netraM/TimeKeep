using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Threading;
using TimeKeep.App.Services;
using TimeKeep.RPC.Categories;

namespace TimeKeep.App.ViewModels;

public class AddCategoryViewModel : ViewModelBase
{
    [Reactive]
    public string Name { get; set; } = string.Empty;

    public ReactiveCommand<Unit, AddCategoryResult> AddCategoryCommand;

    public AddCategoryViewModel()
    {
        AddCategoryCommand = ReactiveCommand.CreateFromTask(AddCategory);
    }

    private async Task<AddCategoryResult> AddCategory(CancellationToken ct)
    {
        try
        {
            var client = await RpcClientFactory.CategoriesClient;
            if (client is null)
            {
                return new AddCategoryFailure("No entry client");
            }
            
            var response = await client.CreateAsync(new CreateRequest { Name = Name }, cancellationToken: ct);

            return response.Status switch
            {
                CreateStatus.Failure => new AddCategoryFailure("Failed (???)"),
                CreateStatus.Success => new AddCategorySuccess(),
                CreateStatus.CategoryExists => new AddCategoryFailure($"The given category ({Name}) already exists."),
                var code => new AddCategoryFailure($"Unknown response code: {code}"),
            };
        }
        catch (Exception e)
        {
            return new AddCategoryFailure("Something went wrong:\n" + e.Message);
        }
    }
}

public abstract record AddCategoryResult;

public record AddCategorySuccess : AddCategoryResult;

public record AddCategoryFailure(string Message) : AddCategoryResult;
