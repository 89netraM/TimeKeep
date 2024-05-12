using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ReactiveUI;
using TimeKeep.App.Services;
using TimeKeep.RPC.Entries;

namespace TimeKeep.App.ViewModels;

public class EntriesViewModel : ViewModelBase, IActivatableViewModel
{
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    private CancellationTokenSource? cancellationTokenSource;

    public ObservableCollection<Entry> ActiveEntries { get; } = new();
    public ObservableCollection<Entry> TodaysEntries { get; } = new();

    public ReactiveCommand<Entry, Unit> DestroyActiveEntryCommand { get; }
    public ReactiveCommand<Entry, Unit> DestroyTodaysEntryCommand { get; }

	public ReactiveCommand<Entry, Unit> EndEntryCommand { get; }

	public ViewModelActivator Activator { get; } = new();

    public EntriesViewModel()
    {
        this.WhenActivated(OnActivation);

        RefreshCommand = ReactiveCommand.CreateFromTask(Refresh);

        DestroyActiveEntryCommand = ReactiveCommand.Create<Entry>(DestroyActiveEntry);
        DestroyTodaysEntryCommand = ReactiveCommand.Create<Entry>(DestroyTodaysEntry);

        EndEntryCommand = ReactiveCommand.Create<Entry>(EndEntry);
    }

    private void OnActivation(CompositeDisposable disposables)
    {
        cancellationTokenSource = new CancellationTokenSource();
        Disposable.Create(cancellationTokenSource.Cancel).DisposeWith(disposables);
        cancellationTokenSource.DisposeWith(disposables);
        LoadEntries(cancellationTokenSource.Token).DisposeWith(disposables);
    }

    private async Task Refresh(CancellationToken ct)
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
        await LoadEntries(cancellationTokenSource.Token);
    }

    private async Task LoadEntries(CancellationToken ct)
    {
        var client = await RpcClientFactory.EntriesClient;
        if (client is null || ct.IsCancellationRequested)
        {
            return;
        }
        await LoadActiveEntries(client, ct);
        if (ct.IsCancellationRequested)
        {
            return;
        }
        await LoadTodaysEntries(client, ct);
    }

    private async Task LoadActiveEntries(EntriesService.EntriesServiceClient client, CancellationToken ct)
    {
        using var request = client.List(new ListRequest { EndStatus = EndStatus.OnlyActive }, cancellationToken: ct);
        Clear(ActiveEntries);
        await foreach (var entry in request.ResponseStream.ReadAllAsync(ct))
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }
            ActiveEntries.Add(entry);
        }
    }

    private async Task LoadTodaysEntries(EntriesService.EntriesServiceClient client, CancellationToken ct)
    {
        var today = DateTime.Today.ToUniversalTime().ToTimestamp();
        using var request = client.List(new ListRequest { After = today, EndStatus = EndStatus.OnlyCompleted }, cancellationToken: ct);
        Clear(TodaysEntries);
        await foreach (var entry in request.ResponseStream.ReadAllAsync(ct))
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }
            TodaysEntries.Add(entry);
        }
    }

    private static void Clear<T>(ObservableCollection<T> collection)
    {
        while (collection.Count > 0)
        {
            collection.RemoveAt(collection.Count - 1);
        }
    }

    private void DestroyActiveEntry(Entry entry) =>
        DestroyEntry(ActiveEntries, entry);
    private void DestroyTodaysEntry(Entry entry) =>
        DestroyEntry(TodaysEntries, entry);
    private void DestroyEntry(ObservableCollection<Entry> entries, Entry entry) =>
        entries.Remove(entry);

	private void EndEntry(Entry entry)
    {
        ActiveEntries.Remove(entry);
        AddEntryToCompletedList(entry);
    }

    private void AddEntryToCompletedList(Entry entry)
    {
        if (entry.Start.ToDateTime().Date == DateTime.Today)
        {
            for (var i = 0; i < TodaysEntries.Count; i++)
            {
                if (entry.Start > TodaysEntries[i].Start)
                {
                    TodaysEntries.Insert(i, entry);
                    return;
                }
            }
            TodaysEntries.Add(entry);
        }
    }
}
