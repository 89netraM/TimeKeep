using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TimeKeep.App.Services;
using TimeKeep.RPC.Entries;

namespace TimeKeep.App.ViewModels;

public class EntriesViewModel : ViewModelBase, IActivatableViewModel
{
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
    private CancellationTokenSource? cancellationTokenSource;

    public ObservableCollection<Entry> ActiveEntries { get; } = new();
    public ObservableCollection<Entry> TodaysEntries { get; } = new();

    [Reactive]
    public TimeSpan TotalWorkTime { get; private set; } = TimeSpan.Zero;

    [Reactive]
    public DateTime? GoHomeTime { get; private set; }

    public ReactiveCommand<Entry, Unit> DestroyActiveEntryCommand { get; }
    public ReactiveCommand<Entry, Unit> DestroyTodaysEntryCommand { get; }

	public ReactiveCommand<Entry, Unit> EndEntryCommand { get; }

	public ViewModelActivator Activator { get; } = new();

    public EntriesViewModel()
    {
        RefreshCommand = ReactiveCommand.CreateFromTask(Refresh);

        DestroyActiveEntryCommand = ReactiveCommand.Create<Entry>(DestroyActiveEntry);
        DestroyTodaysEntryCommand = ReactiveCommand.Create<Entry>(DestroyTodaysEntry);

        EndEntryCommand = ReactiveCommand.Create<Entry>(EndEntry);
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
        await LoadEntries(client, ct);
    }

    private async Task LoadEntries(EntriesService.EntriesServiceClient client, CancellationToken ct)
    {
        var now = DateTime.Now;
        var today = now.Date;
        var duration = new Common.Duration();

        using var request = client.List(
            new ListRequest { After = today.ToUniversalTime().ToTimestamp(), },
            cancellationToken: ct
        );

        Clear(ActiveEntries);
        Clear(TodaysEntries);

        await foreach (var entry in request.ResponseStream.ReadAllAsync(ct))
        {
            if (entry.End is null)
            {
                ActiveEntries.Add(entry);
            }
            else
            {
                TodaysEntries.Add(entry);
            }

            if (entry.Categories.Contains("work"))
            {
                var start = entry.Start.ToDateTimeOffset().LocalDateTime;
                duration.AddInterval(
                    start < today ? today : start,
                    entry.End?.ToDateTimeOffset().LocalDateTime ?? now
                );
            }

            if (ct.IsCancellationRequested)
            {
                break;
            }
        }

        TotalWorkTime = duration.TotalDuration;
        GoHomeTime = now + TimeSpan.FromHours(8) - duration.TotalDuration;
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
